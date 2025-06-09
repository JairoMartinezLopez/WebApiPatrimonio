using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using WebApiPatrimonio.Services;

[ApiController]
[Route("pantallas")]
public class PantallasController : ControllerBase
{
    private readonly IConfiguration _cfg;

    public PantallasController(IConfiguration cfg)
    {
        _cfg = cfg;
    }

    [HttpGet("sidebar")]
    public IActionResult GetPantallasSidebar()
    {
        using var cnn = new SqlConnection(_cfg.GetConnectionString("Conexion"));

        var pantallas = cnn.Query(
            """
            SELECT Pantalla AS Ruta,
                   ISNULL(NombreMenu, Pantalla) AS Nombre
            FROM   CAT_PANTALLAS_CFG
            WHERE  Activo = 1 AND MostrarSidebar = 1
            ORDER BY Orden;
            """
        );

        return Ok(pantallas);
    }
}
