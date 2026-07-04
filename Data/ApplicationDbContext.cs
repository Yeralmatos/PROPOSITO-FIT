using Microsoft.EntityFrameworkCore;
using PropositoFit.Models;

namespace PropositoFit.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        public DbSet<Objetivo> Objetivos { get; set; }

        public DbSet<Rutina> Rutinas { get; set; }
    }
}