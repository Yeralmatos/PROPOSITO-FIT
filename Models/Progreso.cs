using System;

namespace PropositoFit.Models
{
    public class Progreso
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int RutinaId { get; set; }

        public int EjerciciosCompletados { get; set; }

        public int TotalEjercicios { get; set; }

        public decimal Porcentaje { get; set; }

        public DateTime Fecha { get; set; } = DateTime.Now;
    }
}
