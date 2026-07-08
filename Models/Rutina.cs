using System;

namespace PropositoFit.Models
{
    public class Rutina
    {
        public int Id { get; set; }

        // Objetivo al que pertenece la rutina
        public int ObjetivoId { get; set; }

        // Nombre de la rutina
        public string Nombre { get; set; } = "";

        // Descripción
        public string Descripcion { get; set; } = "";

        // Nivel de dificultad
        public string Nivel { get; set; } = "";

        // Instrucciones generales
        public string Instrucciones { get; set; } = "";

        // Estado
        public string Estado { get; set; } = "Activa";

        // Fecha de creación
        public DateTime FechaCreacion { get; set; } = DateTime.Now;
    }
}
