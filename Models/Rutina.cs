using System;

namespace PropositoFit.Models
{
    public class Rutina
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int ObjetivoId { get; set; }

        public string Nombre { get; set; } = "";

        public string Descripcion { get; set; } = "";

        public string DiaSemana { get; set; } = "";

        public int DuracionMinutos { get; set; }

        public string Nivel { get; set; } = "";

        public bool Completada { get; set; }

        public DateTime FechaCreacion { get; set; }

        public DateTime? FechaCompletada { get; set; }

        public string Instrucciones { get; set; } = "";
    }
}