using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PropositoFit.Data;
using PropositoFit.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

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
        public IActionResult Objetivos()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivos = _context.Objetivos
                .Where(o => o.UsuarioId == usuarioId.Value)
                .OrderByDescending(o => o.Id)
                .ToList();

            return View(objetivos);
        }

        [HttpPost]
        public IActionResult CrearObjetivo(Objetivo objetivo)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            bool tieneObjetivoActivo = _context.Objetivos
                .Any(o => o.UsuarioId == usuarioId.Value && o.Estado != "Completado");

            if (tieneObjetivoActivo)
            {
                TempData["ErrorObjetivo"] = "Ya tienes un objetivo activo. Complétalo antes de crear uno nuevo.";
                return RedirectToAction("Objetivos");
            }

            objetivo.UsuarioId = usuarioId.Value;
            objetivo.Descripcion = objetivo.Descripcion ?? "";
            objetivo.MetaObjetivo = objetivo.MetaObjetivo ?? "";
            objetivo.Prioridad = objetivo.Prioridad ?? "Media";
            objetivo.Progreso = 0;
            objetivo.Estado = "En Progreso";

            _context.Objetivos.Add(objetivo);
            _context.SaveChanges();

            var rutinas = CrearRutinasPorObjetivo(usuarioId.Value, objetivo.Id, objetivo.TipoObjetivo);

            if (rutinas.Any())
            {
                _context.Rutinas.AddRange(rutinas);
                _context.SaveChanges();
            }

            return RedirectToAction("Objetivos");
        }

        [HttpPost]
        public IActionResult CompletarObjetivo(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivo = _context.Objetivos
                .FirstOrDefault(o => o.Id == id && o.UsuarioId == usuarioId.Value);

            if (objetivo != null)
            {
                objetivo.Progreso = 100;
                objetivo.Estado = "Completado";
                _context.SaveChanges();
            }

            return RedirectToAction("Objetivos");
        }

        [HttpPost]
        public IActionResult EliminarObjetivo(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            _context.Database.ExecuteSqlRaw(
                "DELETE FROM Rutinas WHERE ObjetivoId = {0} AND UsuarioId = {1}",
                id,
                usuarioId.Value
            );

            _context.Database.ExecuteSqlRaw(
                "DELETE FROM Objetivos WHERE Id = {0} AND UsuarioId = {1}",
                id,
                usuarioId.Value
            );

            return RedirectToAction("Objetivos");
        }

        public IActionResult Rutinas()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivoActual = _context.Objetivos
                .Where(o => o.UsuarioId == usuarioId.Value && o.Estado != "Completado")
                .OrderByDescending(o => o.Id)
                .FirstOrDefault();

            if (objetivoActual == null)
            {
                ViewBag.ObjetivoActual = null;
                return View(new List<Rutina>());
            }

            var rutinas = _context.Rutinas
                .Where(r => r.UsuarioId == usuarioId.Value && r.ObjetivoId == objetivoActual.Id)
                .OrderBy(r => r.Id)
                .ToList();

            ViewBag.ObjetivoActual = objetivoActual.TipoObjetivo;

            return View(rutinas);
        }

        [HttpPost]
        public IActionResult CompletarRutina(int id)
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var rutina = _context.Rutinas
                .FirstOrDefault(r => r.Id == id && r.UsuarioId == usuarioId.Value);

            if (rutina != null && !rutina.Completada)
            {
                rutina.Completada = true;
                rutina.FechaCompletada = DateTime.Now;

                var totalRutinas = _context.Rutinas
                    .Count(r => r.ObjetivoId == rutina.ObjetivoId && r.UsuarioId == usuarioId.Value);

                var completadas = _context.Rutinas
                    .Count(r => r.ObjetivoId == rutina.ObjetivoId &&
                                r.UsuarioId == usuarioId.Value &&
                                r.Completada);

                var objetivo = _context.Objetivos
                    .FirstOrDefault(o => o.Id == rutina.ObjetivoId && o.UsuarioId == usuarioId.Value);

                if (objetivo != null && totalRutinas > 0)
                {
                    objetivo.Progreso = (completadas * 100) / totalRutinas;
                    objetivo.Estado = objetivo.Progreso >= 100 ? "Completado" : "En Progreso";
                }

                _context.SaveChanges();
            }

            return RedirectToAction("Rutinas");
        }

        private List<Rutina> CrearRutinasPorObjetivo(int usuarioId, int objetivoId, string tipoObjetivo)
        {
            var rutinas = new List<Rutina>();

            if (tipoObjetivo == "Bajar de peso")
            {
                rutinas.AddRange(new[]
                {
                    NuevaRutina(usuarioId, objetivoId, "Lunes", "Caminar en casa", "Actividad suave para comenzar a quemar calorías.", 25, "Principiante", "Camina dentro de casa o en el patio durante 25 minutos."),
                    NuevaRutina(usuarioId, objetivoId, "Miércoles", "Sentadillas y abdomen", "Ejercicios sencillos para activar el cuerpo.", 25, "Principiante", "Haz 3 rondas: 12 sentadillas, 15 abdominales y 30 segundos de plancha."),
                    NuevaRutina(usuarioId, objetivoId, "Viernes", "Trote suave en el lugar", "Rutina para mejorar la quema de grasa.", 20, "Principiante", "Trota suavemente en el mismo lugar durante 1 minuto y descansa 30 segundos. Repite 10 veces.")
                });
            }
            else if (tipoObjetivo == "Ganar músculo")
            {
                rutinas.AddRange(new[]
                {
                    NuevaRutina(usuarioId, objetivoId, "Lunes", "Flexiones de pecho", "Ayuda a fortalecer pecho, brazos y hombros.", 25, "Principiante", "Haz 4 series de 8 a 12 flexiones. Si es difícil, apoya las rodillas."),
                    NuevaRutina(usuarioId, objetivoId, "Miércoles", "Piernas y glúteos", "Fortalece piernas y glúteos sin usar máquinas.", 30, "Principiante", "Haz 4 series de 15 sentadillas, 12 zancadas por pierna y 15 puentes de glúteos."),
                    NuevaRutina(usuarioId, objetivoId, "Viernes", "Abdomen y fuerza", "Fortalece el centro del cuerpo.", 25, "Principiante", "Haz 3 series de 15 abdominales, 12 elevaciones de piernas y 30 segundos de plancha.")
                });
            }
            else if (tipoObjetivo == "Tonificar el cuerpo")
            {
                rutinas.AddRange(new[]
                {
                    NuevaRutina(usuarioId, objetivoId, "Lunes", "Piernas y abdomen", "Ayuda a fortalecer y dar forma al cuerpo.", 25, "Principiante", "Haz 3 series de 15 sentadillas, 15 abdominales y 30 segundos de plancha."),
                    NuevaRutina(usuarioId, objetivoId, "Miércoles", "Brazos y pecho", "Ejercicios suaves para definir la parte superior.", 25, "Principiante", "Haz 3 series de 10 flexiones con rodillas apoyadas y 15 fondos usando una silla firme."),
                    NuevaRutina(usuarioId, objetivoId, "Viernes", "Rutina general de fortalecimiento", "Trabajo general con ejercicios sencillos.", 30, "Principiante", "Haz 3 rondas: 12 sentadillas, 10 flexiones, 15 abdominales y 20 segundos de plancha.")
                });
            }

            return rutinas;
        }

        private Rutina NuevaRutina(int usuarioId, int objetivoId, string dia, string nombre, string descripcion, int duracion, string nivel, string instrucciones)
        {
            return new Rutina
            {
                UsuarioId = usuarioId,
                ObjetivoId = objetivoId,
                DiaSemana = dia,
                Nombre = nombre,
                Descripcion = descripcion,
                DuracionMinutos = duracion,
                Nivel = nivel,
                Completada = false,
                FechaCreacion = DateTime.Now,
                Instrucciones = instrucciones
            };
        }

        public IActionResult Alimentacion()
        {
            int? usuarioId = HttpContext.Session.GetInt32("UsuarioId");

            if (usuarioId == null)
                return RedirectToAction("Login", "Acceso");

            var objetivoActual = _context.Objetivos
                .Where(o => o.UsuarioId == usuarioId.Value && o.Estado != "Completado")
                .OrderByDescending(o => o.Id)
                .FirstOrDefault();

            if (objetivoActual == null)
            {
                ViewBag.ObjetivoActual = "Sin objetivo";
                ViewBag.Desayuno = "Primero debes crear un objetivo.";
                ViewBag.Almuerzo = "Primero debes crear un objetivo.";
                ViewBag.Cena = "Primero debes crear un objetivo.";
                ViewBag.Agua = "Primero debes crear un objetivo.";

                return View();
            }

            ViewBag.ObjetivoActual = objetivoActual.TipoObjetivo;

            switch (objetivoActual.TipoObjetivo)
            {
                case "Bajar de peso":
                    ViewBag.Desayuno = "Avena, huevo hervido y una fruta.";
                    ViewBag.Almuerzo = "Pollo a la plancha, ensalada y arroz moderado.";
                    ViewBag.Cena = "Atún o pollo con vegetales.";
                    ViewBag.Agua = "2.5 litros diarios.";
                    break;

                case "Ganar músculo":
                    ViewBag.Desayuno = "Huevos, avena, guineo y leche.";
                    ViewBag.Almuerzo = "Arroz, pollo, habichuelas y ensalada.";
                    ViewBag.Cena = "Carne magra, batata o víveres.";
                    ViewBag.Agua = "3 litros diarios.";
                    break;

                case "Tonificar el cuerpo":
                    ViewBag.Desayuno = "Yogur natural, frutas y avena.";
                    ViewBag.Almuerzo = "Pescado, arroz integral y ensalada.";
                    ViewBag.Cena = "Pechuga y vegetales.";
                    ViewBag.Agua = "2.5 litros diarios.";
                    break;

                case "Mejorar condición física":
                    ViewBag.Desayuno = "Pan integral, huevo y frutas.";
                    ViewBag.Almuerzo = "Pollo, arroz y vegetales.";
                    ViewBag.Cena = "Sopa ligera o proteína.";
                    ViewBag.Agua = "2.5 litros diarios.";
                    break;

                case "Aumentar de peso":
                    ViewBag.Desayuno = "Avena con leche, huevos y pan integral.";
                    ViewBag.Almuerzo = "Arroz, pollo, habichuelas y aguacate.";
                    ViewBag.Cena = "Pasta, carne o batata.";
                    ViewBag.Agua = "3 litros diarios.";
                    break;

                case "Mantenerse en forma":
                    ViewBag.Desayuno = "Frutas, huevo y avena.";
                    ViewBag.Almuerzo = "Proteína, arroz moderado y ensalada.";
                    ViewBag.Cena = "Pescado o pollo con vegetales.";
                    ViewBag.Agua = "2 litros diarios.";
                    break;

                case "Vida saludable":
                    ViewBag.Desayuno = "Frutas, avena y agua.";
                    ViewBag.Almuerzo = "Comida balanceada.";
                    ViewBag.Cena = "Cena ligera con vegetales.";
                    ViewBag.Agua = "2 litros diarios.";
                    break;

                case "Mejorar flexibilidad":
                    ViewBag.Desayuno = "Frutas, yogur y avena.";
                    ViewBag.Almuerzo = "Pescado, vegetales y arroz.";
                    ViewBag.Cena = "Ensalada y proteína ligera.";
                    ViewBag.Agua = "2.5 litros diarios.";
                    break;

                default:
                    ViewBag.Desayuno = "Alimentación balanceada.";
                    ViewBag.Almuerzo = "Proteína, vegetales y carbohidratos.";
                    ViewBag.Cena = "Cena ligera.";
                    ViewBag.Agua = "2 litros diarios.";
                    break;
            }

            return View();
        }

        public IActionResult Progreso()
        {
            return View();
        }

        public IActionResult MiPerfil()
        {
            return View();
        }
    }
}