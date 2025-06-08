using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Azure.Core;
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
    public class RegionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RegionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Regiones
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Regiones>>> GetREGIONES()
        {
            return await _context.REGIONES.ToListAsync();
        }

        // GET: api/Regiones/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Regiones>> GetRegiones(int? id)
        {
            var regiones = await _context.REGIONES.FindAsync(id);

            if (regiones == null)
            {
                return NotFound();
            }

            return regiones;
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Regiones>>> FiltrarRegiones(
            [FromQuery] string? clave,
            [FromQuery] string? nombre,
            [FromQuery] int? idgeneral,
            [FromQuery] bool? activo,
            [FromQuery] bool? bloqueado
            
        )
        {
            var query = _context.REGIONES.AsQueryable();

            if (!string.IsNullOrEmpty(clave))
                query = query.Where(f => f.Clave.Contains(clave));

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(f => f.Nombre.Contains(nombre));

            if (idgeneral.HasValue)
                query = query.Where(f => f.idGeneral == idgeneral);

            if (activo.HasValue)
                query = query.Where(f => f.Activo == activo);

            if (bloqueado.HasValue)
                query = query.Where(f => f.Bloqueado == bloqueado);

            var areas = await query
                .Select(f => new Regiones
                {
                    idRegion = f.idRegion,
                    Clave = f.Clave,
                    Nombre = f.Nombre,
                    idGeneral = f.idRegion,
                    Activo = f.Activo,
                    Bloqueado = f.Bloqueado
                })
                .ToListAsync();

            return Ok(areas);
        }

        // PUT: api/Regiones/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutRegiones([FromBody] Regiones request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_REGIONES";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 6));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1115)); // Este es el usuario logueado
            command.Parameters.Add(new SqlParameter("@IdGeneralAsignado", request.idGeneral)); //Este es el usuario asignado a la región
            command.Parameters.Add(new SqlParameter("@idRegion", request.idRegion));
            command.Parameters.Add(new SqlParameter("@Clave", (object?)request.Clave ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Activo", (object?)request.Activo ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Bloqueado", (object?)request.Bloqueado ?? DBNull.Value));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Region actualizada correctamente." });
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

        // POST: api/Regiones
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Regiones>> PostRegiones(Regiones request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_INS_REGIONES";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 4));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1115)); // Este es el usuario logueado
            command.Parameters.Add(new SqlParameter("@IdGeneralAsignado", request.idGeneral)); //Este es el usuario asignado a la región
            command.Parameters.Add(new SqlParameter("@Clave", (object?)request.Clave ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Activo", (object?)request.Activo ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Bloqueado", (object?)request.Bloqueado ?? DBNull.Value));
            
            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Region agregada correctamente." });
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

        // DELETE: api/Regiones/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRegiones(int? id)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_DEL_REGIONES";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1)); // Este es el usuario logueado
            command.Parameters.Add(new SqlParameter("@idRegion", id));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Region eliminada (borrado lógico) correctamente." });
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

        private bool RegionesExists(int? id)
        {
            return _context.REGIONES.Any(e => e.idRegion == id);
        }
    }
}
