using System;

namespace PropositoFit.Models
{
    public class RutinaEjercicio
    {
        public int Id { get; set; }

        public int RutinaId { get; set; }

        public int EjercicioId { get; set; }

        // Orden en que aparece el ejercicio dentro de la rutina
        public int Orden { get; set; }

        // Configuración del ejercicio
        public int Series { get; set; }

        public int Repeticiones { get; set; }

        public int DescansoSegundos { get; set; }

        // Progreso del usuario
        public bool Completado { get; set; } = false;

        public DateTime? FechaCompletado { get; set; }
    }
}
