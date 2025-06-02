using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WebApiPatrimonio.Context;
using WebApiPatrimonio.Middleware;
using WebApiPatrimonio.Services;
using Newtonsoft.Json.Linq;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// ─────────── Kestrel (50 MB) ───────────
builder.WebHost.ConfigureKestrel(o => o.Limits.MaxRequestBodySize = 52_428_800);

// ─────────── DbContext ───────────
var cnn = builder.Configuration.GetConnectionString("Conexion")
          ?? throw new InvalidOperationException("Connection string 'Conexion' no encontrada");
builder.Services.AddDbContext<ApplicationDbContext>(opt => opt.UseSqlServer(cnn));

// ─────────── Servicios propios ───────────
builder.Services.AddScoped<ICatService, CatService>();
builder.Services.AddHttpContextAccessor();

// ─────────── Controllers / Swagger ───────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Inventario API", Version = "v1" });

    c.AddSecurityDefinition("bearerAuth", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Escribe: **Bearer &lt;token&gt;**"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "bearerAuth"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─────────── CORS ───────────
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("CorsPolicy", p =>
        p.AllowAnyOrigin()
         .AllowAnyMethod()
         .AllowAnyHeader());
});

// ─────────── JWT ───────────
var jwt = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwt["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            ValidateLifetime = true
        };
    });

var app = builder.Build();

// ─────────── Middleware global ───────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<JwtMiddleware>();

// ─────────── Controllers regulares ───────────
app.MapControllers();


// ╔════════════════════════════════════════════╗
// ║   ENDPOINTS DINÁMICOS DE CATÁLOGOS        ║
// ╚════════════════════════════════════════════╝

app.MapGet("/meta/{pantalla}",
           (string pantalla, ICatService s) => s.GetColumnsAsync(pantalla));

app.MapGet("/catalogo/{pantalla}",
           (string pantalla, HttpRequest req, ICatService s)
               => s.GetRowsAsync(pantalla, req.Query));

app.MapPost("/catalogo/{pantalla}",
async (string pantalla, JsonElement body, ICatService s) =>
{
    try
    {
        var jo = JObject.Parse(body.GetRawText());
        await s.UpsertAsync(pantalla, jo);
        return Results.Ok(new { ok = true });
    }
    catch (SqlException ex)
    {
        return Results.BadRequest(new { sql = ex.Message });
    }
    catch (UnauthorizedAccessException)
    {
        return Results.Unauthorized();
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapDelete("/catalogo/{pantalla}/{pk}/{id}",
    async (string pantalla, string pk, int id, ICatService s) =>
    {
        try
        {
            await s.DeleteAsync(pantalla, pk, id);
            return Results.Ok(new { ok = true });
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (SqlException ex)
        {
            return Results.BadRequest(new { sql = ex.Message });
        }
        catch (Exception ex)
        {
            return Results.Problem(ex.Message);
        }
    });


app.MapPatch("/toggle/{pantalla}/{pk}/{id}/{col}",
             (string pantalla, string pk, int id, string col, ICatService s)
                 => s.ToggleAsync(pantalla, pk, id, col));

app.MapGet("/foraneas/{pantalla}",
           (string pantalla, ICatService s) => s.GetForaneasAsync(pantalla));


// ─────────── Run ───────────
app.Run();
