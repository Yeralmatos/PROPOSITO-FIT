namespace PropositoFit.Models
{
    public class ReporteUsuarioViewModel
    {
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }

        // Métricas de resumen
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }

        // Lista detallada para la tabla de reportes
        public List<UsuarioReporteItem> DetalleUsuarios { get; set; } = new List<UsuarioReporteItem>();
    }

    public class UsuarioReporteItem
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string Estado { get; set; } = string.Empty;
    }
}