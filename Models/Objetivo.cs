using System;

namespace PropositoFit.Models
{
    public class Objetivo
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = "";

        public string Descripcion { get; set; } = "";

        public string Prioridad { get; set; } = "Media";

        public string Estado { get; set; } = "Activo";

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        public string PalabrasClave { get; set; } = "";
    }
}