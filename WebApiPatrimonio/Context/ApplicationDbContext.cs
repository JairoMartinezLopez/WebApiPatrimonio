using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Context
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<Bien> BIENES { get; set; }
        public DbSet<Areas> AREAS { get; set; }
        public DbSet<Regiones> REGIONES { get; set; }
        public DbSet<Transferencia> TRANSFERENCIAS { get; set; }
        public DbSet<UbicacionFisica> UBICACIONESFISICAS { get; set; }
        public DbSet<Levantamiento> LEVANTAMIENTOSINVENTARIO { get; set; }
        public DbSet<ProgramarEventos> EVENTOSINVENTARIO { get; set; }
        public DbSet<ConfiguracionGeneral> ConfiguracionGeneral { get; set; }


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
        public DbSet<CambiarPassword> CambiarPasswordRequest { get; set; }
        public DbSet<LoginRequest> LoginRequest { get; set; } = default!;

        public DbSet<Color> COLORES { get; set; }
        public DbSet<EstadosFisicos> EstadosFisicos { get; set; }
        public DbSet<Marca> MARCAS { get; set; }
        public DbSet<CausalBajas> CausalBajas { get; set; }
        public DbSet<DisposicionFinal> DISPOSICIONESFINALES { get; set; }
        public DbSet<TipoBien> TIPOSBIENES {  get; set; }
        public DbSet<CatBien> CatalogoBienes { get; set; }
        public DbSet<Financiamiento> FINANCIAMIENTOS { get; set; }
        public DbSet<CatEstados> CATESADOS { get; set; }
        public DbSet<Factura> FACTURAS { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
    }
}
