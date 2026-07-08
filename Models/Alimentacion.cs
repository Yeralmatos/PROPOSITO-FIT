using System.ComponentModel.DataAnnotations;

namespace PropositoFit.Models
{
    public class Alimentacion
    {
        public int Id { get; set; }

        public int ObjetivoId { get; set; }

        [Required]
        public string TipoComida { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        public int Calorias { get; set; }

        public decimal Proteinas { get; set; }

        public decimal Carbohidratos { get; set; }

        public decimal Grasas { get; set; }

        public string Estado { get; set; } = "Activo";
    }
}

