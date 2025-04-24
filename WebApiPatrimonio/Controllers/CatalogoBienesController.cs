using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CatalogoBienesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CatalogoBienesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CatalogoBienes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatBien>>> GetCatBienes()
        {
            return await _context.CatalogoBienes.ToListAsync();
        }

        // GET: api/CatalogoBienes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CatBien>> GetCatBien(int id)
        {
            var catBien = await _context.CatalogoBienes.FindAsync(id);

            if (catBien == null)
            {
                return NotFound();
            }

            return catBien;
        }

        // PUT: api/CatalogoBienes/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpGet("filtar")]
        public async Task<ActionResult<IEnumerable<CatBien>>> filtrarCatalogoBien(
           [FromQuery] int? idTipoBien,
           [FromQuery] string? nombre,
           [FromQuery] string? descripcion,
           [FromQuery] int? clave,
           [FromQuery] bool? activo,
           [FromQuery] bool? bloqueado)
        {
            var query = _context.CatalogoBienes.AsQueryable();

            if (idTipoBien.HasValue)
                query = query.Where(u => u.idTipoBien == idTipoBien);

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(u => u.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(descripcion))
                query = query.Where(u => u.Descripcion.Contains(descripcion));

            if (clave.HasValue)
                query = query.Where(u => u.Clave == clave);

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo);

            if (bloqueado.HasValue)
                query = query.Where(u => u.Bloqueado == bloqueado);

            var CatBienes = await query
                .Select(u => new CatBien
                {
                    idCatalogoBien = u.idCatalogoBien,
                    idTipoBien = u.idTipoBien,
                    Clave = u.Clave,
                    Nombre = u.Nombre,
                    Descripcion = u.Descripcion,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();

            return Ok(CatBienes);
        }

        // PUT: api/CatBien/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("modificar")]
        public async Task<IActionResult> PutCatalogoBien([FromBody] CatBien request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_CAT_BIENES";

            command.Parameters.Add(new SqlParameter("@idCatalogoBien", request.idCatalogoBien));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));//loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@Clave", request.Clave ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idTipoBien", request.idTipoBien));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Catalogo de bien modificada correctamente." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        // POST: api/CatalogosBien
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CatBien>> PostCatalogoBien(CatBien request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "PA_INS_CAT_BIENES";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));//loggedInUserId));
            command.Parameters.Add(new SqlParameter("@idTipoBien", request.idTipoBien));
            command.Parameters.Add(new SqlParameter("@Clave", request.Clave));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Catalogo de bien agregado correctamente." });
            }
            catch (SqlException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }

        // DELETE: api/CatalogoBien/5
        [HttpDelete("{idCatBien}")]
        public async Task<IActionResult> DeleteCatBien(int idCatBien)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            var sql = "EXEC PA_DEL_CAT_BIENES @idCatalogoBien, @IdPantalla, @IdGeneral";
            var result = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idCatalogoBien", idCatBien),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1) //loggedInUserId));
            );

            return Ok(new { mensaje = "Catalogo bien eliminada lógicamente." });
        }

        private bool CatBienExists(int id)
        {
            return _context.CatalogoBienes.Any(e => e.idCatalogoBien == id);
        }
    }
}
