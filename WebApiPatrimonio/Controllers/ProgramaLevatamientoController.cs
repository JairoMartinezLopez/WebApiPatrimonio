using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgramaLevatamientoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProgramaLevatamientoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ProgramaLevatamiento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProgramaLevatamiento>>> GetPAT_EVENTOINVENTARIO()
        {
            return await _context.EVENTOSINVENTARIO.ToListAsync();
        }

        // GET: api/ProgramaLevatamiento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProgramaLevatamiento>> GetProgramaLevatamiento(int id)
        {
            var programaLevatamiento = await _context.EVENTOSINVENTARIO.FindAsync(id);

            if (programaLevatamiento == null)
            {
                return NotFound();
            }

            return programaLevatamiento;
        }

        // PUT: api/ProgramaLevatamiento/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProgramaLevatamiento(int id, ProgramaLevatamiento programaLevatamiento)
        {
            if (id != programaLevatamiento.IdEventoInventario)
            {
                return BadRequest();
            }

            _context.Entry(programaLevatamiento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProgramaLevatamientoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProgramaLevatamiento
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<IActionResult> InsertarEventoInventario([FromBody] EventoInventarioRequest request)
        {
            if (request == null) return BadRequest("Datos inválidos");

            int idEventoInventario = await _context.InsertarEventoInventario(
                request.IdGeneral, request.IdAreaSistemaUsuario, request.IdPantalla,
                request.FechaInicio, request.FechaTermino, request.IdArea,
                request.IdAreaSistemaUsuario2, request.IdEventoEstado);

            return Ok(new { IdEventoInventario = idEventoInventario });
        }

        // DELETE: api/ProgramaLevatamiento/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProgramaLevatamiento(int id)
        {
            var programaLevatamiento = await _context.EVENTOSINVENTARIO.FindAsync(id);
            if (programaLevatamiento == null)
            {
                return NotFound();
            }

            _context.EVENTOSINVENTARIO.Remove(programaLevatamiento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProgramaLevatamientoExists(int id)
        {
            return _context.EVENTOSINVENTARIO.Any(e => e.IdEventoInventario == id);
        }
    }
}
