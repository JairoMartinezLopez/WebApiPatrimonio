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
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios
        [HttpGet("Obtener todo")]
        public async Task<ActionResult<IEnumerable<UsuarioRequest>>> GetUsuarios()
        {
            var usuarios = await _context.USUARIOS
                .Select(u => new UsuarioModel
                {
                    idUsuarios = u.idUsuarios,
                    Nombre = u.Nombre,
                    Apellidos = u.Apellidos,
                    idGeneral = u.idGeneral,
                    Rol = u.Rol,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();
            return Ok(usuarios);
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioRequest>> GetUsuarioRequest(int id)
        {
            var usuarioRequest = await _context.UsuariosRequest.FindAsync(id);

            if (usuarioRequest == null)
            {
                return NotFound();
            }

            return usuarioRequest;
        }

        [HttpGet("filtrar")]
        public async Task<ActionResult<IEnumerable<UsuarioRequest>>> FiltrarUsuarios(
            [FromQuery] string? nombre,
            [FromQuery] int? idGeneral,
            [FromQuery] string? rol,
            [FromQuery] bool? activo,
            [FromQuery] bool? bloqueado)
        {
            var query = _context.USUARIOS.AsQueryable();

            if (!string.IsNullOrEmpty(nombre))
                query = query.Where(u => u.Nombre.Contains(nombre));

            if (idGeneral.HasValue)
                query = query.Where(u => u.idGeneral == idGeneral);

            if (!string.IsNullOrEmpty(rol))
                query = query.Where(u => u.Rol == rol);

            if (activo.HasValue)
                query = query.Where(u => u.Activo == activo);

            if (bloqueado.HasValue)
                query = query.Where(u => u.Bloqueado == bloqueado);

            var usuarios = await query
                .Select(u => new UsuarioRequest
                {
                    idUsuarios = u.idUsuarios,
                    Nombre = u.Nombre,
                    Apellidos = u.Apellidos,
                    idGeneral = u.idGeneral,
                    Rol = u.Rol,
                    Activo = u.Activo,
                    Bloqueado = u.Bloqueado
                })
                .ToListAsync();

            return Ok(usuarios);
        }


        // PUT: api/Usuarios/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("modificar")]
        public async Task<IActionResult> ModificarUsuario([FromBody] UsuarioRequest request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_UPD_USUARIOS";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@idUsuarios", request.idUsuarios));
            command.Parameters.Add(new SqlParameter("@IdPantalla", request.idPantalla));
            command.Parameters.Add(new SqlParameter("@idGeneral", request.idGeneral));
            command.Parameters.Add(new SqlParameter("@Rol", request.Rol));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Usuario modificado correctamente." });
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


        // POST: api/Usuarios
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754

        [HttpPost("Agregar Usuario")]
        public async Task<IActionResult> InsertarUsuario([FromBody] UsuarioRequest request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_INS_USUARIOS";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@idUsuarios", request.idUsuarios));
            command.Parameters.Add(new SqlParameter("@IdPantalla", request.idPantalla));
            command.Parameters.Add(new SqlParameter("@Nombre", request.Nombre));
            command.Parameters.Add(new SqlParameter("@Apellidos", request.Apellidos));
            command.Parameters.Add(new SqlParameter("@Password", request.Password));
            command.Parameters.Add(new SqlParameter("@idGeneral", request.idGeneral));
            command.Parameters.Add(new SqlParameter("@Rol", request.Rol));
            command.Parameters.Add(new SqlParameter("@Activo", request.Activo));
            command.Parameters.Add(new SqlParameter("@Bloqueado", request.Bloqueado));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();
                return Ok(new { mensaje = "Usuario insertado correctamente." });
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


        // DELETE: api/Usuarios/5
        [HttpDelete]
        public async Task<IActionResult> EliminarUsuario([FromBody] UsuarioRequest request)
        {
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "PA_DEL_USUARIO";
            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.Add(new SqlParameter("@idUsuarios", request.idUsuarios));
            command.Parameters.Add(new SqlParameter("@IdPantalla", request.idPantalla));
            command.Parameters.Add(new SqlParameter("@idGeneral", request.idGeneral));

            try
            {
                await _context.Database.OpenConnectionAsync();
                await command.ExecuteNonQueryAsync();

                return Ok(new { mensaje = "Usuario desactivado correctamente." });
            }
            catch (SqlException ex)
            {
                return StatusCode(500, new { error = "Error al desactivar el usuario.", detalle = ex.Message });
            }
            finally
            {
                await _context.Database.CloseConnectionAsync();
            }
        }


        private bool UsuarioRequestExists(int id)
        {
            return _context.UsuariosRequest.Any(e => e.idUsuarios == id);
        }
    }
}
