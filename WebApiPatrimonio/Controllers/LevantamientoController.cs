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
    public class LevantamientoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LevantamientoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Levantamiento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Levantamiento>>> GetPAT_LEVANTAMIENTO_INVENTARIO()
        {
            return await _context.LEVANTAMIENTOSINVENTARIO.ToListAsync();
        }

        // GET: api/Levantamiento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Levantamiento>> GetLevantamiento(long id)
        {
            var levantamiento = await _context.LEVANTAMIENTOSINVENTARIO.FindAsync(id);

            if (levantamiento == null)
            {
                return NotFound();
            }

            return levantamiento;
        }

        // PUT: api/Levantamiento/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLevantamiento(long id, Levantamiento levantamiento)
        {
            if (id != levantamiento.IdLevantamientoInventario)
            {
                return BadRequest();
            }

            _context.Entry(levantamiento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LevantamientoExists(id))
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

        // POST: api/Levantamiento
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Levantamiento>> PostLevantamiento(Levantamiento levantamiento)
        {
            _context.LEVANTAMIENTOSINVENTARIO.Add(levantamiento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLevantamiento", new { id = levantamiento.IdLevantamientoInventario }, levantamiento);
        }

        // DELETE: api/Levantamiento/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLevantamiento(long id)
        {
            var levantamiento = await _context.LEVANTAMIENTOSINVENTARIO.FindAsync(id);
            if (levantamiento == null)
            {
                return NotFound();
            }

            _context.LEVANTAMIENTOSINVENTARIO.Remove(levantamiento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LevantamientoExists(long id)
        {
            return _context.LEVANTAMIENTOSINVENTARIO.Any(e => e.IdLevantamientoInventario == id);
        }
    }
}
