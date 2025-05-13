using Microsoft.AspNetCore.Mvc;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfiguracionController : ControllerBase
    {
        private readonly ApplicationDbContext _ctx;
        public ConfiguracionController(ApplicationDbContext ctx) => _ctx = ctx;

        [HttpGet]
        public async Task<ActionResult<ConfiguracionGeneral>> Get()
        {
            var cfg = await _ctx.ConfiguracionGeneral.FindAsync(1);
            if (cfg == null)                         // no se encontró registro
                return NotFound();                   // 404

            return cfg;                              // 200 + JSON (Ok() implícito)
        }


        [HttpPut]
        public async Task<IActionResult> Put([FromBody] ConfiguracionGeneral dto)
        {
            var cfg = await _ctx.ConfiguracionGeneral.FindAsync(1);
            if (cfg is null) return NotFound();
            cfg.GuardarEnNas = dto.GuardarEnNas;
            await _ctx.SaveChangesAsync();
            return NoContent();
        }
    }
}
