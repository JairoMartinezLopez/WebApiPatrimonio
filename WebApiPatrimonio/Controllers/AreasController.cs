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
    public class AreasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AreasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Areas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Areas>>> GetAREAS()
        {
            return await _context.AREAS.ToListAsync();
        }

        // GET: api/Areas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Areas>> GetAreas(int? id)
        {
            var areas = await _context.AREAS.FindAsync(id);

            if (areas == null)
            {
                return NotFound();
            }

            return areas;
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<Areas>>> FiltrarAreas(
            [FromQuery] string? clave,
            [FromQuery] string? nombre,
            [FromQuery] string? direccion,
            [FromQuery] int? idareapadre,
            [FromQuery] bool? activo,
            [FromQuery] int? idUnidadresponsable,
            [FromQuery] bool? permitirentradas,
            [FromQuery] bool? permitirsalidas,
            [FromQuery] int? idregion
        )
        {
            var query = _context.AREAS.AsQueryable();

            if (!string.IsNullOrEmpty(clave))
                query = query.Where(f => f.Clave.Contains(clave));

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(f => f.Nombre.Contains(nombre));

            if (!string.IsNullOrEmpty(direccion))
                query = query.Where(f => f.Direccion.Contains(direccion));

            if (idareapadre.HasValue)
                query = query.Where(f => f.idAreaPadre == idareapadre);

            if (activo.HasValue)
                query = query.Where(f => f.Activo == activo);

            if (idUnidadresponsable.HasValue)
                query = query.Where(f => f.idUnidadResponsable == idUnidadresponsable);
            
            if (permitirentradas.HasValue)
                query = query.Where(f => f.PermitirEntradas == permitirentradas);

            if (permitirsalidas.HasValue)
                query = query.Where(f => f.PermitirSalidas == permitirsalidas);

            if (idregion.HasValue)
                query = query.Where(f => f.idRegion == idregion);

            var areas = await query
                .Select(f => new Areas
                {
                    idArea = f.idArea,
                    Clave = f.Clave,
                    Nombre = f.Nombre,
                    Direccion = f.Direccion,
                    idAreaPadre = f.idAreaPadre,
                    Activo = f.Activo,
                    idUnidadResponsable = f.idUnidadResponsable,
                    PermitirEntradas = f.PermitirEntradas,
                    PermitirSalidas = f.PermitirSalidas,
                    idRegion = f.idRegion
                })
                .ToListAsync();

            return Ok(areas);
        }

        // PUT: api/Areas/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut]
        public async Task<IActionResult> PutAreas([FromBody] Areas request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_UPD_AREAS";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 6));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 6));
            command.Parameters.Add(new SqlParameter("@idArea", request.idArea));
            command.Parameters.Add(new SqlParameter("@Clave", (object?)request.Clave ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Direccion", (object?)request.Direccion ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idAreaPadre", (object?)request.idAreaPadre ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Activo", (object?)request.Activo ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@IdUnidadResponsable", request.idUnidadResponsable));
            command.Parameters.Add(new SqlParameter("@PermitirEntradas", (object?)request.PermitirEntradas ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@PermitirSalidas", (object?)request.PermitirSalidas ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idRegion", request.idRegion));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Área actualizada correctamente." });
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

        // POST: api/Areas
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> PostArea([FromBody] Areas request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_INS_AREAS";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 4));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 4));
            command.Parameters.Add(new SqlParameter("@Clave", (object?)request.Clave ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Direccion", (object?)request.Direccion ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idAreaPadre", (object?)request.idAreaPadre ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@IdUnidadResponsable", request.idUnidadResponsable));
            command.Parameters.Add(new SqlParameter("@PermitirEntradas", (object?)request.PermitirEntradas ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@PermitirSalidas", (object?)request.PermitirSalidas ?? DBNull.Value));
            command.Parameters.Add(new SqlParameter("@idRegion", request.idRegion));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Área agregada correctamente." });
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

        // DELETE: api/Areas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAreas(int? id)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandType = CommandType.StoredProcedure;
            command.CommandText = "PA_DEL_AREAS";

            command.Parameters.Add(new SqlParameter("@IdPantalla", 1));
            command.Parameters.Add(new SqlParameter("@IdGeneral", 1));
            command.Parameters.Add(new SqlParameter("@idArea", id));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Area eliminada (borrado lógico) correctamente." });
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

        private bool AreasExists(int? id)
        {
            return _context.AREAS.Any(e => e.idArea == id);
        }
    }
}
