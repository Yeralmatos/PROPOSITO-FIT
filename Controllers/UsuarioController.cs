using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PropositoFit.Data;
using PropositoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PropositoFit.Controllers
{
    public class UsuarioController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuarioController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            return View();
        }

        [HttpGet]
        public IActionResult Objetivos(string buscar)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivoUsuario = _context.UsuarioObjetivos
                .Where(uo => uo.UsuarioId == usuarioId.Value && uo.Estado == "Activo")
                .OrderByDescending(uo => uo.Id)
                .FirstOrDefault();

            Objetivo? objetivoActual = null;

            if (objetivoUsuario != null)
            {
                objetivoActual = _context.Objetivos
                    .FirstOrDefault(o => o.Id == objetivoUsuario.ObjetivoId);
            }

            ViewBag.Buscar = buscar;
            ViewBag.ObjetivoUsuario = objetivoUsuario;
            ViewBag.ObjetivoActual = objetivoActual;

            return View(new List<Objetivo>());
        }

        [HttpGet]
        public IActionResult BuscarObjetivosAjax(string termino)
        {
            if (string.IsNullOrWhiteSpace(termino))
                return Json(new List<object>());

            termino = termino.Trim();

            var palabras = termino.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var objetivos = _context.Objetivos
                .ToList()
                .Where(o =>
                    o.Estado == "Activo" &&
                    palabras.Any(p =>
                        (o.Nombre ?? "").Contains(p, StringComparison.OrdinalIgnoreCase) ||
                        (o.Descripcion ?? "").Contains(p, StringComparison.OrdinalIgnoreCase) ||
                        (o.PalabrasClave ?? "").Contains(p, StringComparison.OrdinalIgnoreCase)))
                .Select(o => new
                {
                    id = o.Id,
                    nombre = o.Nombre ?? "",
                    descripcion = o.Descripcion ?? ""
                })
                .Take(8)
                .ToList();

            return Json(objetivos);
        }

        [HttpPost]
        public IActionResult SeleccionarObjetivo(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivo = _context.Objetivos
                .ToList()
                .FirstOrDefault(o => o.Id == id && o.Estado == "Activo");

            if (objetivo == null)
                return RedirectToAction("Objetivos");

            return RedirectToAction("ConfigurarObjetivo", new { id = objetivo.Id });
        }

        [HttpGet]
        public IActionResult ConfigurarObjetivo(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivo = _context.Objetivos
                .ToList()
                .FirstOrDefault(o => o.Id == id && o.Estado == "Activo");

            if (objetivo == null)
                return RedirectToAction("Objetivos");

            ViewBag.Objetivo = objetivo;

            return View();
        }

        [HttpPost]
        public IActionResult GuardarObjetivo(UsuarioObjetivo modelo)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivosActivos = _context.UsuarioObjetivos
                .Where(uo => uo.UsuarioId == usuarioId.Value && uo.Estado == "Activo")
                .ToList();

            foreach (var item in objetivosActivos)
            {
                item.Estado = "Inactivo";
            }

            modelo.UsuarioId = usuarioId.Value;
            modelo.Estado = "Activo";
            modelo.FechaRegistro = DateTime.Now;

            _context.UsuarioObjetivos.Add(modelo);
            _context.SaveChanges();

            return RedirectToAction("Rutinas", new { objetivoId = modelo.ObjetivoId });
        }

        [HttpPost]
        public IActionResult EliminarObjetivoActual()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivoUsuario = _context.UsuarioObjetivos
                .Where(uo => uo.UsuarioId == usuarioId.Value && uo.Estado == "Activo")
                .OrderByDescending(uo => uo.Id)
                .FirstOrDefault();

            if (objetivoUsuario != null)
            {
                objetivoUsuario.Estado = "Inactivo";
                _context.SaveChanges();
            }

            return RedirectToAction("Objetivos");
        }

        [HttpGet]
        public IActionResult Rutinas(int? objetivoId, string buscar)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivoUsuario = _context.UsuarioObjetivos
                .Where(uo => uo.UsuarioId == usuarioId.Value && uo.Estado == "Activo")
                .OrderByDescending(uo => uo.Id)
                .FirstOrDefault();

            if (objetivoId == null && objetivoUsuario == null)
            {
                ViewBag.SinObjetivo = true;
                ViewBag.RutinaEjercicios = new List<RutinaEjercicio>();
                ViewBag.Ejercicios = new List<Ejercicio>();
                ViewBag.Completados = new List<EjercicioCompletado>();

                return View(new List<Rutina>());
            }

            int objetivoSeleccionadoId = objetivoId ?? objetivoUsuario!.ObjetivoId;

            var objetivo = _context.Objetivos
                .FirstOrDefault(o => o.Id == objetivoSeleccionadoId);

            ViewBag.ObjetivoActual = objetivo?.Nombre ?? "Objetivo seleccionado";
            ViewBag.ObjetivoId = objetivoSeleccionadoId;

            var listaRutinas = _context.Rutinas
                .Where(r => r.ObjetivoId == objetivoSeleccionadoId)
                .ToList()
                .Where(r =>
                    r.Estado == "Activa" &&
                    (
                        string.IsNullOrWhiteSpace(buscar) ||
                        (r.Nombre ?? "").Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                        (r.Descripcion ?? "").Contains(buscar, StringComparison.OrdinalIgnoreCase) ||
                        (r.Nivel ?? "").Contains(buscar, StringComparison.OrdinalIgnoreCase)
                    ))
                .OrderBy(r => r.Nombre ?? "")
                .ToList();

            var rutinaIds = listaRutinas.Select(r => r.Id).ToList();

            var rutinaEjercicios = _context.RutinaEjercicios
                .Where(re => rutinaIds.Contains(re.RutinaId))
                .OrderBy(re => re.Orden)
                .ToList();

            var ejerciciosIds = rutinaEjercicios.Select(re => re.EjercicioId).ToList();
            var ejercicios = _context.Ejercicios
     .Where(e => ejerciciosIds.Contains(e.Id))
     .Select(e => new Ejercicio
     {
         Id = e.Id,
         ObjetivoId = e.ObjetivoId,
         Nombre = e.Nombre ?? "",
         GrupoMuscular = e.GrupoMuscular ?? "",
         Categoria = e.Categoria ?? "",
         Descripcion = e.Descripcion ?? "",
         ComoSeHace = e.ComoSeHace ?? "",
         Series = e.Series,
         Repeticiones = e.Repeticiones,
         DescansoSegundos = e.DescansoSegundos,
         Nivel = e.Nivel ?? "",
         Gif = e.Gif ?? "",
         Estado = e.Estado ?? ""
     })
     .Where(e => e.Estado == "Activo")
     .ToList();


            var completados = _context.EjerciciosCompletados
                .Where(ec => ec.UsuarioId == usuarioId.Value && rutinaIds.Contains(ec.RutinaId))
                .ToList();

            ViewBag.Buscar = buscar;
            ViewBag.RutinaEjercicios = rutinaEjercicios;
            ViewBag.Ejercicios = ejercicios;
            ViewBag.Completados = completados;

            return View(listaRutinas);
        }

        [HttpPost]
        public IActionResult CompletarEjercicio(int rutinaId, int ejercicioId)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            bool yaExiste = _context.EjerciciosCompletados.Any(ec =>
                ec.UsuarioId == usuarioId.Value &&
                ec.RutinaId == rutinaId &&
                ec.EjercicioId == ejercicioId);

            if (!yaExiste)
            {
                _context.EjerciciosCompletados.Add(new EjercicioCompletado
                {
                    UsuarioId = usuarioId.Value,
                    RutinaId = rutinaId,
                    EjercicioId = ejercicioId,
                    FechaCompletado = DateTime.Now
                });

                _context.SaveChanges();
            }

            return RedirectToAction("Rutinas");
        }

        [HttpPost]
        public IActionResult CompletarRutina(int rutinaId)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var ejerciciosRutina = _context.RutinaEjercicios
                .Where(re => re.RutinaId == rutinaId)
                .ToList();

            foreach (var item in ejerciciosRutina)
            {
                bool yaExiste = _context.EjerciciosCompletados.Any(ec =>
                    ec.UsuarioId == usuarioId.Value &&
                    ec.RutinaId == rutinaId &&
                    ec.EjercicioId == item.EjercicioId);

                if (!yaExiste)
                {
                    _context.EjerciciosCompletados.Add(new EjercicioCompletado
                    {
                        UsuarioId = usuarioId.Value,
                        RutinaId = rutinaId,
                        EjercicioId = item.EjercicioId,
                        FechaCompletado = DateTime.Now
                    });
                }
            }

            _context.SaveChanges();

            return RedirectToAction("Rutinas");
        }

        public IActionResult Alimentacion()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivoUsuario = _context.UsuarioObjetivos
                .Where(uo => uo.UsuarioId == usuarioId.Value && uo.Estado == "Activo")
                .OrderByDescending(uo => uo.Id)
                .FirstOrDefault();

            if (objetivoUsuario == null)
            {
                ViewBag.SinObjetivo = true;
                ViewBag.ObjetivoActual = "Sin objetivo";
                ViewBag.Desayunos = new List<Alimentacion>();
                ViewBag.Almuerzos = new List<Alimentacion>();
                ViewBag.Cenas = new List<Alimentacion>();
                ViewBag.Meriendas = new List<Alimentacion>();

                return View();
            }

            var objetivo = _context.Objetivos
                .FirstOrDefault(o => o.Id == objetivoUsuario.ObjetivoId);

            var alimentaciones = _context.Alimentaciones
                .ToList()
                .Where(a => a.ObjetivoId == objetivoUsuario.ObjetivoId && a.Estado == "Activo")
                .ToList();

            ViewBag.SinObjetivo = false;
            ViewBag.ObjetivoActual = objetivo?.Nombre ?? "Objetivo seleccionado";

            ViewBag.Desayunos = alimentaciones.Where(a => a.TipoComida == "Desayuno").ToList();
            ViewBag.Almuerzos = alimentaciones.Where(a => a.TipoComida == "Almuerzo").ToList();
            ViewBag.Cenas = alimentaciones.Where(a => a.TipoComida == "Cena").ToList();
            ViewBag.Meriendas = alimentaciones.Where(a => a.TipoComida == "Merienda").ToList();

            return View();
        }

        [HttpPost]
        public IActionResult GuardarRegistroAlimentacion(int? alimentacionId, string comida, int calorias, decimal agua)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            if (string.IsNullOrWhiteSpace(comida))
                return RedirectToAction("Alimentacion");

            _context.RegistrosAlimentacion.Add(new RegistroAlimentacion
            {
                UsuarioId = usuarioId.Value,
                AlimentacionId = alimentacionId,
                ComidaConsumida = comida,
                Calorias = calorias,
                AguaLitros = agua,
                FechaRegistro = DateTime.Now
            });

            _context.SaveChanges();

            return RedirectToAction("Alimentacion");
        }

        public IActionResult Progreso()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivoUsuario = _context.UsuarioObjetivos
                .Where(uo => uo.UsuarioId == usuarioId.Value && uo.Estado == "Activo")
                .OrderByDescending(uo => uo.Id)
                .FirstOrDefault();

            if (objetivoUsuario == null)
            {
                ViewBag.ObjetivoActual = "Sin objetivo";
                ViewBag.TotalEjercicios = 0;
                ViewBag.EjerciciosCompletados = 0;
                ViewBag.EjerciciosRestantes = 0;
                ViewBag.Porcentaje = 0;
                ViewBag.RutinasCompletadas = 0;
                ViewBag.TotalRutinas = 0;
                ViewBag.UltimoEntrenamiento = "Sin registros";

                return View();
            }

            var objetivo = _context.Objetivos
                .FirstOrDefault(o => o.Id == objetivoUsuario.ObjetivoId);

            var rutinas = _context.Rutinas
                .Where(r => r.ObjetivoId == objetivoUsuario.ObjetivoId)
                .ToList()
                .Where(r => r.Estado == "Activa")
                .ToList();

            var rutinaIds = rutinas.Select(r => r.Id).ToList();

            var rutinaEjercicios = _context.RutinaEjercicios
                .Where(re => rutinaIds.Contains(re.RutinaId))
                .ToList();

            var completados = _context.EjerciciosCompletados
                .Where(ec => ec.UsuarioId == usuarioId.Value && rutinaIds.Contains(ec.RutinaId))
                .ToList();

            int totalEjercicios = rutinaEjercicios.Count;
            int ejerciciosCompletados = completados.Count;

            int porcentaje = totalEjercicios == 0
                ? 0
                : (int)Math.Round((ejerciciosCompletados * 100.0) / totalEjercicios);

            int rutinasCompletadas = rutinas.Count(r =>
            {
                var ejerciciosRutina = rutinaEjercicios
                    .Where(re => re.RutinaId == r.Id)
                    .ToList();

                return ejerciciosRutina.Count > 0 &&
                       ejerciciosRutina.All(re =>
                           completados.Any(c =>
                               c.RutinaId == r.Id &&
                               c.EjercicioId == re.EjercicioId));
            });

            var ultimo = completados
                .OrderByDescending(c => c.FechaCompletado)
                .FirstOrDefault();

            ViewBag.ObjetivoActual = objetivo?.Nombre ?? "Objetivo seleccionado";
            ViewBag.TotalEjercicios = totalEjercicios;
            ViewBag.EjerciciosCompletados = ejerciciosCompletados;
            ViewBag.EjerciciosRestantes = totalEjercicios - ejerciciosCompletados;
            ViewBag.Porcentaje = porcentaje;
            ViewBag.RutinasCompletadas = rutinasCompletadas;
            ViewBag.TotalRutinas = rutinas.Count;
            ViewBag.UltimoEntrenamiento = ultimo == null
                ? "Sin registros"
                : ultimo.FechaCompletado.ToString("dd/MM/yyyy hh:mm tt");

            ViewBag.Rutinas = rutinas;
            ViewBag.RutinaEjercicios = rutinaEjercicios;
            ViewBag.Completados = completados;

            return View();
        }

        public IActionResult MiPerfil()
        {
            return View();
        }
    }
}
