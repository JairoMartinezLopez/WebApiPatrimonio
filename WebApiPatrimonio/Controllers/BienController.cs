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
    public class BienController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public BienController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Bien
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bien>>> GetPAT_BIENES()
        {
            return await _context.PAT_BIENES.ToListAsync();
        }

        // GET: api/Bien/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bien>> GetBien(long id)
        {
            var bien = await _context.PAT_BIENES.FindAsync(id);

            if (bien == null)
            {
                return NotFound();
            }

            return bien;
        }

        // PUT: api/Bien/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBien(long id, Bien bien)
        {
            if (id != bien.IdBien)
            {
                return BadRequest();
            }

            _context.Entry(bien).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BienExists(id))
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

        // POST: api/Bien
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Bien>> PostBien(Bien bien)
        {
            _context.PAT_BIENES.Add(bien);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBien", new { id = bien.IdBien }, bien);
        }

        // DELETE: api/Bien/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBien(long id)
        {
            var bien = await _context.PAT_BIENES.FindAsync(id);
            if (bien == null)
            {
                return NotFound();
            }

            _context.PAT_BIENES.Remove(bien);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BienExists(long id)
        {
            return _context.PAT_BIENES.Any(e => e.IdBien == id);
        }
    }
}
