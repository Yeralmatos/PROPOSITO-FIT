using System;

namespace PropositoFit.Models
{
    public class RegistroAlimentacion
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        public int? AlimentacionId { get; set; }

        public string ComidaConsumida { get; set; }

        public int Calorias { get; set; }

        public decimal AguaLitros { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}

