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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===========================
            // TABLA USUARIOS
            // ===========================

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Correo)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.NombreUsuario)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Estado)
                .HasDefaultValue("Activo");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.Rol)
                .HasDefaultValue("Cliente");

            modelBuilder.Entity<Usuario>()
                .Property(u => u.FechaRegistro)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}