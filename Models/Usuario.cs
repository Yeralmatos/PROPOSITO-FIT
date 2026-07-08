using System;

namespace PropositoFit.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        public string Nombre { get; set; }
        public string Cedula { get; set; } = "";
        public string Correo { get; set; }

        public string Contrasena { get; set; }

        public int? Edad { get; set; }

        public string Sexo { get; set; }

        public decimal? Peso { get; set; }

        public decimal? Estatura { get; set; }

        public DateTime? FechaRegistro { get; set; }

        public string Rol { get; set; }
    }
}