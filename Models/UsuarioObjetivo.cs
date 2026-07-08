using System;

namespace PropositoFit.Models
{
    public class UsuarioObjetivo
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int ObjetivoId { get; set; }

        public string? DescripcionPersonal { get; set; }

        public DateTime FechaInicio { get; set; }

        public DateTime FechaMeta { get; set; }

        public string? MetaEspecifica { get; set; }

        public string? Prioridad { get; set; }

        public string? Estado { get; set; }

        public DateTime FechaRegistro { get; set; }
    }
}

