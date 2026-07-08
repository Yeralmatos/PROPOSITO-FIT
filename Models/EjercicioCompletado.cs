using System;

namespace PropositoFit.Models
{
    public class EjercicioCompletado
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int RutinaId { get; set; }

        public int EjercicioId { get; set; }

        public DateTime FechaCompletado { get; set; } = DateTime.Now;
    }
}