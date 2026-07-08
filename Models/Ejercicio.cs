namespace PropositoFit.Models
{
    public class Ejercicio
    {
        public int Id { get; set; }

        public string? Nombre { get; set; }

        public int? ObjetivoId { get; set; }

        public string? GrupoMuscular { get; set; }

        public string? Categoria { get; set; }

        public string? Descripcion { get; set; }

        public string? ComoSeHace { get; set; }

        public int? Series { get; set; }

        public int? Repeticiones { get; set; }

        public int? DescansoSegundos { get; set; }

        public string? Nivel { get; set; }

        public string? Gif { get; set; }

        public string? Estado { get; set; }
    }
}

