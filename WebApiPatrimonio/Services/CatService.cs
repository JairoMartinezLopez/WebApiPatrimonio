using System.Data;
using System.Security.Claims;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace WebApiPatrimonio.Services;

/* ─────────────────────────────────────────────
   ¡OJO!  ListItem ya está declarado en ICatService.
   Por eso lo quitamos aquí para evitar la colisión.
   ───────────────────────────────────────────── */

public class CatService : ICatService
{
    private readonly IConfiguration _cfg;
    private readonly IHttpContextAccessor _http;

    /* ───── Config de pantallas ───── */
    private readonly Dictionary<string, PantallaCfg> _pantallas;

    private record PantallaCfg(
        string Pantalla,
        string ObjetoSQL,
        string Tipo,
        string? Pk,
        string? SpIns,
        string? SpUpd,
        string? SpDel);

    public CatService(IConfiguration cfg, IHttpContextAccessor http)
    {
        _cfg = cfg;
        _http = http;

        using var cnn = new SqlConnection(_cfg.GetConnectionString("Conexion"));
        _pantallas = cnn.Query<PantallaCfg>(
            """
            SELECT Pantalla,
                   ObjetoSQL,
                   Tipo,
                   ISNULL(PkName   ,'') AS Pk,
                   ISNULL(SpInsert ,'') AS SpIns,
                   ISNULL(SpUpdate ,'') AS SpUpd,
                   ISNULL(SpDelete ,'') AS SpDel
            FROM   CAT_PANTALLAS_CFG
            WHERE  Activo = 1
            """)
            .ToDictionary(p => p.Pantalla, p => p, StringComparer.OrdinalIgnoreCase);
    }

    /* ───────────────── helpers ───────────────── */
    private int GetUserId()
    {
        var claim = _http.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || !int.TryParse(claim.Value, out int id))
            throw new UnauthorizedAccessException("Usuario no autenticado");
        return id;
    }

    /* ─────────── Listas foráneas ─────────── */
    public async Task<Dictionary<string, List<ListItem>>> GetForaneasAsync(string pantalla)
    {
        const string sqlRel = """
            SELECT Columna, TablaDestino, Valor, Texto
            FROM   CAT_PANTALLAS_FORANEAS
            WHERE  Activo = 1 AND Pantalla = @pantalla;
        """;

        await using var cnn = new SqlConnection(_cfg.GetConnectionString("Conexion"));
        var rels = await cnn.QueryAsync(sqlRel, new { pantalla });

        var result = new Dictionary<string, List<ListItem>>(StringComparer.OrdinalIgnoreCase);

        foreach (var rel in rels)
        {
            var datos = await cnn.QueryAsync($"""
                SELECT [{rel.Valor}] AS Val, [{rel.Texto}] AS Lab
                FROM   [{rel.TablaDestino}]
                WHERE  Activo = 1 AND Bloqueado = 0;
            """);

            result[(string)rel.Columna] = datos
                .Select(r => new ListItem(r.Val, r.Lab))
                .ToList();
        }
        return result;
    }

    /* ─────────── GetColumnsAsync ─────────── */
    public async Task<IEnumerable<ColMeta>> GetColumnsAsync(string pantalla)
    {
        if (!_pantallas.TryGetValue(pantalla, out var cfg))
            throw new ArgumentException("Pantalla no registrada");

        await using var cnn = new SqlConnection(_cfg.GetConnectionString("Conexion"));

        if (cfg.Tipo is "TABL" or "VIST")
        {
            const string sql = """
                SELECT c.name                 AS Name,
                       t.name                 AS DataType,
                       c.is_nullable          AS IsNullable,
                       CAST(i.is_primary_key AS bit) AS IsPK,
                       c.is_identity          AS IsIdentity
                FROM   sys.columns c
                JOIN   sys.types   t ON c.user_type_id = t.user_type_id
                LEFT  JOIN sys.index_columns ic
                           ON ic.object_id = c.object_id AND ic.column_id = c.column_id
                LEFT  JOIN sys.indexes i
                           ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                           AND i.is_primary_key = 1
                WHERE  c.object_id = OBJECT_ID(@obj)
                ORDER BY c.column_id;
            """;
            return await cnn.QueryAsync<ColMeta>(sql, new { obj = cfg.ObjetoSQL });
        }

        const string dmv = """
            SELECT name              AS Name,
                   system_type_name  AS DataType,
                   is_nullable       AS IsNullable,
                   CAST(0 AS bit)    AS IsPK,
                   CAST(0 AS bit)    AS IsIdentity
            FROM sys.dm_exec_describe_first_result_set
                 (N'EXEC ' + @sp, NULL, 0);
        """;
        return await cnn.QueryAsync<ColMeta>(dmv, new { sp = cfg.ObjetoSQL });
    }

    /* ─────────── GetRowsAsync (sin cambios) ─────────── */
    public async Task<IEnumerable<IDictionary<string, object>>> GetRowsAsync(string pantalla, IQueryCollection qs)
    {
        if (!_pantallas.TryGetValue(pantalla, out var cfg))
            throw new ArgumentException("Pantalla no registrada");

        await using var cnn = new SqlConnection(_cfg.GetConnectionString("Conexion"));

        if (cfg.Tipo == "PROC")
        {
            var p = new DynamicParameters();
            foreach (var (k, v) in qs) p.Add($"@{k}", v.ToString());

            var rows = await cnn.QueryAsync(cfg.ObjetoSQL, p, commandType: CommandType.StoredProcedure);
            return rows.Select(r => (IDictionary<string, object>)r).ToList();
        }

        var cols = await GetColumnsAsync(pantalla);
        var where = new List<string>();
        var dp = new DynamicParameters();

        foreach (var (k, v) in qs)
            if (cols.Any(c => c.Name.Equals(k, StringComparison.OrdinalIgnoreCase)))
            {
                where.Add($"{k} = @{k}");
                dp.Add($"@{k}", v.ToString());
            }

        var sql = $"SELECT * FROM [{cfg.ObjetoSQL}]" +
                  (where.Count > 0 ? " WHERE " + string.Join(" AND ", where) : "");

        var res = await cnn.QueryAsync(sql, dp);
        return res.Select(r => (IDictionary<string, object>)r).ToList();
    }

    public async Task UpsertAsync(string pantalla, JObject payload)
    {
        if (!_pantallas.TryGetValue(pantalla, out var cfg))
            throw new ArgumentException("Pantalla no registrada");

        if (string.IsNullOrWhiteSpace(cfg.SpIns) || string.IsNullOrWhiteSpace(cfg.SpUpd))
            throw new InvalidOperationException("La pantalla es solo lectura");

        string pkName = cfg.Pk ?? "";
        bool esAlta = string.IsNullOrEmpty(pkName) ||
                        !payload.TryGetValue(pkName, out var pkTok) ||
                        pkTok!.Value<int>() == 0;

        string sp = esAlta ? cfg.SpIns! : cfg.SpUpd!;

        var d = new DynamicParameters();
        foreach (var prop in payload)
            d.Add($"@{prop.Key}", prop.Value?.ToObject<object>() ?? DBNull.Value);

        if (!d.ParameterNames.Contains("@IdPantalla")) d.Add("@IdPantalla", 1);
        if (!d.ParameterNames.Contains("@IdGeneral")) d.Add("@IdGeneral", GetUserId());

        await using var cnn = new SqlConnection(_cfg.GetConnectionString("Conexion"));
        await cnn.ExecuteAsync(sp, d, commandType: CommandType.StoredProcedure);
    }

    public async Task ToggleAsync(string pantalla, string pkName, int id, string column)
    {
        if (!_pantallas.TryGetValue(pantalla, out var cfg))
            throw new ArgumentException("Pantalla no registrada");

        if (cfg.Tipo == "PROC")
            throw new InvalidOperationException("Pantalla PROC es solo lectura");

        string sql = $"""
            UPDATE [{cfg.ObjetoSQL}]
            SET {column} = IIF({column}=1, 0, 1)
            WHERE {pkName} = @id;
        """;

        await using var cnn = new SqlConnection(_cfg.GetConnectionString("Conexion"));
        await cnn.ExecuteAsync(sql, new { id });
    }

    //  NUEVO MÉTODO DELETE
    public async Task DeleteAsync(string pantalla, string pkName, int id)
    {
        if (!_pantallas.TryGetValue(pantalla, out var cfg))
            throw new ArgumentException("Pantalla no registrada");

        await using var cnn = new SqlConnection(_cfg.GetConnectionString("Conexion"));

        if (!string.IsNullOrWhiteSpace(cfg.SpDel))
        {
            var d = new DynamicParameters();
            d.Add($"@{pkName}", id);
            d.Add("@IdPantalla", 1);
            d.Add("@IdGeneral", GetUserId());

            await cnn.ExecuteAsync(cfg.SpDel, d, commandType: CommandType.StoredProcedure);
        }
        else if (cfg.Tipo != "PROC")
        {
            string sql = $"DELETE FROM [{cfg.ObjetoSQL}] WHERE {pkName} = @id";
            await cnn.ExecuteAsync(sql, new { id });
        }
        else
        {
            throw new InvalidOperationException("No se puede eliminar: pantalla de solo lectura");
        }
    }

}
