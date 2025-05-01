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
    public class CatEstadosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CatEstadosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/CatEstados
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CatEstados>>> GetCATESADOS()
        {
            return await _context.CATESADOS.ToListAsync();
        }

        // GET: api/CatEstados/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CatEstados>> GetCatEstados(int id)
        {
            var catEstados = await _context.CATESADOS.FindAsync(id);

            if (catEstados == null)
            {
                return NotFound();
            }

            return catEstados;
        }

        // PUT: api/CatEstados/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpGet("filtar")]
        public async Task<ActionResult<IEnumerable<CatEstados>>> filtrarCatEstados(
           [FromQuery] string? nombre,
           [FromQuery] string? descripcion,
           [FromQuery] string? clave,
           [FromQuery] bool? activo,
           [FromQuery] bool? bloqueado)
        {
            var query = _context.CATESADOS.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(u => u.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(descripcion))
                query = query.Where(u => u.Descripcion.Contains(descripcion));

            if (!string.IsNullOrEmpty(clave))
                query = query.Where(u => u.Clave.Contains(clave));

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo);

            if (bloqueado.HasValue)
                query = query.Where(u => u.Bloqueado == bloqueado);

            var CatsEstados = await query
                .Select(u => new CatEstados
                {
                    idEstado = u.idEstado,
                    Clave = u.Clave,
                    Nombre = u.Nombre,
                    Descripcion = u.Descripcion,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();

            return Ok(CatsEstados);
        }

        // PUT: api/CatEstados/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("modificar")]
        public async Task<IActionResult> PutCatEstados([FromBody] CatEstados request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_CAT_ESTADOS";

            command.Parameters.Add(new SqlParameter("@idEstado", request.idEstado));
            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));//loggedInUserId)); // Usar el ID del usuario autenticado
            command.Parameters.Add(new SqlParameter("@Clave", request.Clave ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion ?? (object)DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Estado modificado correctamente." });
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

        // POST: api/CatEstados
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CatEstados>> PostCatEstados(CatEstados request)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = System.Data.CommandType.StoredProcedure;
            command.CommandText = "PA_INS_CAT_ESTADOS";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1)); // Reemplaza con el ID de pantalla adecuado
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));//loggedInUserId));
            command.Parameters.Add(new SqlParameter("@Clave", request.Clave));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Descripcion", request.Descripcion));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Estado agregado correctamente." });
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

        // DELETE: api/CatEstados/5
        [HttpDelete("{idEstado}")]
        public async Task<IActionResult> DeleteCatFinanciamientos(int idEstado)
        {
            // Obtener el ID del usuario autenticado
            /*var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int loggedInUserId))
            {
                return Unauthorized(new { error = "Usuario no autenticado o ID de usuario no válido." });
            }*/

            var sql = "EXEC PA_DEL_CAT_ESTADOS @idEstado, @IdPantalla, @IdGeneral";
            var result = await _context.Database.ExecuteSqlRawAsync(sql,
                new SqlParameter("@idEstado", idEstado),
                new SqlParameter("@IdPantalla", 1),
                new SqlParameter("@IdGeneral", 1) //loggedInUserId));
            );

            return Ok(new { mensaje = "Estado eliminada lógicamente." });
        }
        private bool CatEstadosExists(int id)
        {
            return _context.CATESADOS.Any(e => e.idEstado == id);
        }
    }
}
