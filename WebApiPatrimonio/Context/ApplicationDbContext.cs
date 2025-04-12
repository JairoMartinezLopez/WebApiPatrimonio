using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Context
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<Bien> BIENES { get; set; }

        public async Task<int> InsertarBienAsync(
            int idGeneral, int idAreaSistemaUsuario, int idPantalla, int idColor,
            DateTime fechaAlta, string aviso, string serie, string modelo, int idEstadoFisico,
            int idMarca, double costo, bool etiquetado, DateTime? fechaEtiquetado, bool estatus,
            int idFactura, string noInventario, int idTipoBien, int idCatBien,
            string observaciones, int idCategoria, int idFinanciamiento, int idAdscripcion,
            string salida, int cantidad)
        {
            int idBien = 0;

            using (var connection = Database.GetDbConnection())
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "PA_INS_PAT_BIENES";
                    command.CommandType = CommandType.StoredProcedure;

                    // Parámetros de entrada
                    command.Parameters.Add(new SqlParameter("@IdGeneral", idGeneral));
                    command.Parameters.Add(new SqlParameter("@IdAreaSistemaUsuario", idAreaSistemaUsuario));
                    command.Parameters.Add(new SqlParameter("@IdPantalla", idPantalla));
                    command.Parameters.Add(new SqlParameter("@idColor", idColor));
                    command.Parameters.Add(new SqlParameter("@FechaAlta", fechaAlta));
                    command.Parameters.Add(new SqlParameter("@Aviso", aviso ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Serie", serie ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Modelo", modelo ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@idEstadoFisico", idEstadoFisico));
                    command.Parameters.Add(new SqlParameter("@idMarca", idMarca));
                    command.Parameters.Add(new SqlParameter("@Costo", costo));
                    command.Parameters.Add(new SqlParameter("@Etiquetado", etiquetado));
                    command.Parameters.Add(new SqlParameter("@FechaEtiquetado", (object)fechaEtiquetado ?? DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Estatus", estatus));
                    command.Parameters.Add(new SqlParameter("@IdFactura", idFactura));
                    command.Parameters.Add(new SqlParameter("@NoInventario", noInventario ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@idTipoBien", idTipoBien));
                    command.Parameters.Add(new SqlParameter("@IdCatBien", idCatBien));
                    command.Parameters.Add(new SqlParameter("@Observaciones", observaciones ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@IdCategoria", idCategoria));
                    command.Parameters.Add(new SqlParameter("@IdFinanciamiento", idFinanciamiento));
                    command.Parameters.Add(new SqlParameter("@IdAdscripcion", idAdscripcion));
                    command.Parameters.Add(new SqlParameter("@Salida", salida ?? (object)DBNull.Value));
                    command.Parameters.Add(new SqlParameter("@Cantidad", cantidad));

                    // Parámetro de salida
                    var outputParam = new SqlParameter("@IdBien", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    command.Parameters.Add(outputParam);

                    await command.ExecuteNonQueryAsync();

                    // Obtener el valor del parámetro de salida
                    idBien = (int)outputParam.Value;
                }
            }

            return idBien;
        }

        public DbSet<Levantamiento> LEVANTAMIENTOSINVENTARIO { get; set; }

        public DbSet<ProgramarEventos> EVENTOSINVENTARIO { get; set; }

        public async Task<int> InsertarEventoInventario(
        int idGeneral, int idAreaSistemaUsuario, int idPantalla, DateTime fechaInicio,
        DateTime fechaTermino, int idArea, int idAreaSistemaUsuario2, int idEventoEstado)
        {
            var idEventoInventarioParam = new SqlParameter
            {
                ParameterName = "@IdEventoInventario",
                SqlDbType = System.Data.SqlDbType.Int,
                Direction = System.Data.ParameterDirection.Output
            };

            await Database.ExecuteSqlRawAsync(
                "EXEC PA_INS_PAT_EVENTOINVENTARIO @IdGeneral, @IdAreaSistemaUsuario, @IdPantalla, @IdEventoInventario OUTPUT, @FechaInicio, @FechaTermino, @idArea, @idAreaSistemaUsuario2, @idEventoEstado",
                new SqlParameter("@IdGeneral", idGeneral),
                new SqlParameter("@IdAreaSistemaUsuario", idAreaSistemaUsuario),
                new SqlParameter("@IdPantalla", idPantalla),
                idEventoInventarioParam,
                new SqlParameter("@FechaInicio", fechaInicio),
                new SqlParameter("@FechaTermino", fechaTermino),
                new SqlParameter("@idArea", idArea),
                new SqlParameter("@idAreaSistemaUsuario2", idAreaSistemaUsuario2),
                new SqlParameter("@idEventoEstado", idEventoEstado)
            );

            return (int)idEventoInventarioParam.Value;
        }
        public DbSet<Roles> ROLES { get; set; }
        public DbSet<Modulo> MODULOS { get; set; }
        public DbSet<Accion> ACCIONES { get; set; }
        public DbSet<UsuarioModel> USUARIOS { get; set; }
        public DbSet<Permiso> PERMISOS { get; set; }
        public DbSet<UsuariosPermiso> UsuariosPermisos { get; set; }
        public DbSet<UsuarioRequest> UsuariosRequest { get; set; }

        public DbSet<LoginRequest> LoginRequest { get; set; } = default!;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
    }
}
