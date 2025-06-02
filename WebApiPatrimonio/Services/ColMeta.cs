namespace WebApiPatrimonio.Services;
using Newtonsoft.Json.Linq;
using System.Data;

public record ColMeta(
    string Name,          // Nombre de la columna
    string DataType,      // varchar, int, bit…
    bool IsNullable,
    bool IsPK,
    bool IsIdentity
);

