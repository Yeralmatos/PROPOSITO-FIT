using System;
using System.ComponentModel.DataAnnotations;

namespace PropositoFit.Models
{
    public class Usuario
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
        [StringLength(150)]
        public string NombreCliente { get; set; } = "";

        [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
        [StringLength(50)]
        public string NombreUsuario { get; set; } = "";

        [Required(ErrorMessage = "La cédula es obligatoria.")]
        public string Cedula { get; set; } = "";

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress]
        public string Correo { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Contrasena { get; set; } = "";

        [Required]
        public DateTime FechaNacimiento { get; set; }

        [Required]
        public string Sexo { get; set; } = "";

        public decimal Peso { get; set; }

        public decimal Estatura { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        public string Estado { get; set; } = "Activo";

        public string Rol { get; set; } = "Cliente";
    }
}