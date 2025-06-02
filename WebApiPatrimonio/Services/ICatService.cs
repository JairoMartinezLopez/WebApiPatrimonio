using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;

namespace WebApiPatrimonio.Services;

/// Item genérico para los combos (value–label)
public record ListItem(object value, string label);

public interface ICatService
{
    /* ───── Metadatos + filas ───── */
    Task<IEnumerable<ColMeta>> GetColumnsAsync(string pantalla);
    Task<IEnumerable<IDictionary<string, object>>> GetRowsAsync(string pantalla, IQueryCollection qs);

    /* ───── CRUD genérico ───── */
    Task UpsertAsync(string pantalla, JObject payload);
    Task ToggleAsync(string pantalla, string pkName, int id, string column);
    Task DeleteAsync(string pantalla, string pkName, int id);

    /* ───── Listas foráneas ───── */
    Task<Dictionary<string, List<ListItem>>> GetForaneasAsync(string pantalla);
}

