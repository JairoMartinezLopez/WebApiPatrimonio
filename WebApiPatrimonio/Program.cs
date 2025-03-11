using Microsoft.EntityFrameworkCore;
using WebApiPatrimonio.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//Variable de conexion a la base de datos
var connectionString = builder.Configuration.GetConnectionString("Conexion")
        ?? throw new InvalidOperationException("Connection string" + "'Conexion' no encontrada.");
//Registar servicio para la conexion
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// habilita CORS para cualquier origen, método y encabezado
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

var app = builder.Build();
app.UseCors("CorsPolicy");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();