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

        public DbSet<Rol> Roles { get; set; }

        public DbSet<Objetivo> Objetivos { get; set; }

        public DbSet<UsuarioObjetivo> UsuarioObjetivos { get; set; }

        public DbSet<Alimentacion> Alimentaciones { get; set; }

        public DbSet<RegistroAlimentacion> RegistrosAlimentacion { get; set; }

        public DbSet<Rutina> Rutinas { get; set; }

        public DbSet<Ejercicio> Ejercicios { get; set; }

        public DbSet<RutinaEjercicio> RutinaEjercicios { get; set; }

        public DbSet<EjercicioCompletado> EjerciciosCompletados { get; set; }

        public DbSet<Progreso> Progresos { get; set; }
    }
}
