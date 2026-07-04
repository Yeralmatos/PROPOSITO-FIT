using System;

namespace PropositoFit.Models
{
    public class Objetivo
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public string TipoObjetivo { get; set; } = "";

        public string Descripcion { get; set; } = "";

        public DateTime FechaInicio { get; set; }

        public DateTime FechaMeta { get; set; }

        public string MetaObjetivo { get; set; } = "";

        public string Prioridad { get; set; } = "";

        public int Progreso { get; set; }

        public string Estado { get; set; } = "";
    }
}