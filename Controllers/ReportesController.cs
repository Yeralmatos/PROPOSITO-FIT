using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropositoFit.Data; // Ajusta este namespace según la carpeta donde esté tu DbContext
using PropositoFit.Models;

namespace PropositoFit.Controllers
{
	public class ReportesController : Controller
	{
		private readonly ApplicationDbContext _context; // Ajusta "ApplicationDbContext" al nombre de tu DbContext

		public ReportesController(ApplicationDbContext context)
		{
			_context = context;
		}

		// Vista principal del reporte
		public async Task<IActionResult> Usuarios(DateTime? fechaInicio, DateTime? fechaFin)
		{
			// Si no selecciona fechas, mostramos los registros del último mes
			var inicio = fechaInicio ?? DateTime.Now.AddMonths(-1);
			var fin = fechaFin ?? DateTime.Now;

			// Filtrar los usuarios desde la base de datos
			var usuariosQuery = _context.Usuarios
				.Where(u => u.FechaRegistro >= inicio && u.FechaRegistro <= fin);

			var model = new ReporteUsuarioViewModel
			{
				FechaInicio = inicio,
				FechaFin = fin,
				TotalUsuarios = await usuariosQuery.CountAsync(),
				UsuariosActivos = await usuariosQuery.CountAsync(u => u.Activo),
				DetalleUsuarios = await usuariosQuery.Select(u => new UsuarioReporteItem
				{
					Id = u.Id,
					Nombre = u.Nombre,
					Email = u.Email,
					FechaRegistro = u.FechaRegistro,
					Estado = u.Activo ? "Activo" : "Inactivo"
				}).ToListAsync()
			};

			return View(model);
		}
	}
}