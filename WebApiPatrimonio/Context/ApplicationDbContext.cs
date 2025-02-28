using Microsoft.EntityFrameworkCore;
using WebApiPatrimonio.Models;

namespace WebApiPatrimonio.Context
{
    public class ApplicationDbContext: DbContext
    {
        public DbSet<Bien> PAT_BIENES { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
    }
}
