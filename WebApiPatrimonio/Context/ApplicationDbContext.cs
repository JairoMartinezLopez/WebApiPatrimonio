using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Context
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<Bien> PAT_BIENES { get; set; }

        public DbSet<Levantamiento> PAT_LEVANTAMIENTO_INVENTARIO { get; set; }

        public DbSet<ProgramaLevatamiento> PAT_EVENTOINVENTARIO { get; set; }

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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
    }
}
