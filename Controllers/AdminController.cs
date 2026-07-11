using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PropositoFit.Data;
using PropositoFit.Models;
using System;
using System.IO;
using System.Linq;

namespace PropositoFit.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            ViewBag.TotalUsuarios = _context.Usuarios.Count();
            ViewBag.TotalEjercicios = _context.Ejercicios.Count();
            ViewBag.TotalRutinas = _context.Rutinas.Count();
            ViewBag.TotalObjetivos = _context.Objetivos.Count();

            return View();
        }

        public IActionResult Usuarios()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");
            return View(_context.Usuarios.ToList());
        }

        [HttpGet]
        public IActionResult NuevoUsuario()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");
            return View();
        }

        [HttpPost]
        public IActionResult NuevoUsuario(Usuario usuario)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            usuario.FechaRegistro = DateTime.Now;
            usuario.Rol = string.IsNullOrWhiteSpace(usuario.Rol) ? "Usuario" : usuario.Rol;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return RedirectToAction("Usuarios");
        }

        [HttpGet]
        public IActionResult EditarUsuario(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);
            if (usuario == null) return RedirectToAction("Usuarios");

            return View(usuario);
        }

        [HttpPost]
        public IActionResult EditarUsuario(Usuario usuario)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var usuarioBD = _context.Usuarios.FirstOrDefault(u => u.Id == usuario.Id);
            if (usuarioBD == null) return RedirectToAction("Usuarios");

            usuarioBD.Nombre = usuario.Nombre;
            usuarioBD.Cedula = usuario.Cedula;
            usuarioBD.Correo = usuario.Correo;
            usuarioBD.Contrasena = usuario.Contrasena;
            usuarioBD.FechaNacimiento = usuario.FechaNacimiento;
            usuarioBD.Sexo = usuario.Sexo;
            usuarioBD.Peso = usuario.Peso;
            usuarioBD.Estatura = usuario.Estatura;
            usuarioBD.Rol = usuario.Rol;

            _context.SaveChanges();

            return RedirectToAction("Usuarios");
        }

        [HttpPost]
        public IActionResult EliminarUsuario(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);

            if (usuario != null)
            {
                var completados = _context.EjerciciosCompletados.Where(e => e.UsuarioId == id).ToList();
                var objetivosUsuario = _context.UsuarioObjetivos.Where(o => o.UsuarioId == id).ToList();

                _context.EjerciciosCompletados.RemoveRange(completados);
                _context.UsuarioObjetivos.RemoveRange(objetivosUsuario);
                _context.Usuarios.Remove(usuario);
                _context.SaveChanges();
            }

            return RedirectToAction("Usuarios");
        }

        public IActionResult Objetivos(string buscar)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var objetivos = _context.Objetivos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                objetivos = objetivos.Where(o =>
                    o.Nombre.Contains(buscar) ||
                    o.Descripcion.Contains(buscar) ||
                    o.PalabrasClave.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(objetivos.OrderByDescending(o => o.Id).ToList());
        }

        [HttpGet]
        public IActionResult NuevoObjetivo()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");
            return View();
        }

        [HttpPost]
        public IActionResult NuevoObjetivo(Objetivo objetivo)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            objetivo.Nombre ??= "";
            objetivo.Descripcion ??= "";
            objetivo.PalabrasClave ??= "";
            objetivo.Prioridad = string.IsNullOrWhiteSpace(objetivo.Prioridad) ? "Media" : objetivo.Prioridad;
            objetivo.Estado = string.IsNullOrWhiteSpace(objetivo.Estado) ? "Activo" : objetivo.Estado;
            objetivo.FechaCreacion = DateTime.Now;

            _context.Objetivos.Add(objetivo);
            _context.SaveChanges();

            return RedirectToAction("Objetivos");
        }

        [HttpGet]
        public IActionResult EditarObjetivo(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var objetivo = _context.Objetivos.FirstOrDefault(o => o.Id == id);
            if (objetivo == null) return RedirectToAction("Objetivos");

            return View(objetivo);
        }

        [HttpPost]
        public IActionResult EditarObjetivo(Objetivo objetivo)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var objetivoBD = _context.Objetivos.FirstOrDefault(o => o.Id == objetivo.Id);
            if (objetivoBD == null) return RedirectToAction("Objetivos");

            objetivoBD.Nombre = objetivo.Nombre ?? "";
            objetivoBD.Descripcion = objetivo.Descripcion ?? "";
            objetivoBD.PalabrasClave = objetivo.PalabrasClave ?? "";
            objetivoBD.Prioridad = objetivo.Prioridad ?? "Media";
            objetivoBD.Estado = objetivo.Estado ?? "Activo";

            _context.SaveChanges();

            return RedirectToAction("Objetivos");
        }

        [HttpPost]
        public IActionResult EliminarObjetivo(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var objetivo = _context.Objetivos.FirstOrDefault(o => o.Id == id);

            if (objetivo != null)
            {
                var rutinas = _context.Rutinas.Where(r => r.ObjetivoId == id).ToList();

                foreach (var rutina in rutinas)
                {
                    var rutinaEjercicios = _context.RutinaEjercicios.Where(re => re.RutinaId == rutina.Id).ToList();
                    var completados = _context.EjerciciosCompletados.Where(ec => ec.RutinaId == rutina.Id).ToList();

                    _context.RutinaEjercicios.RemoveRange(rutinaEjercicios);
                    _context.EjerciciosCompletados.RemoveRange(completados);
                }

                var objetivosUsuario = _context.UsuarioObjetivos.Where(uo => uo.ObjetivoId == id).ToList();

                _context.UsuarioObjetivos.RemoveRange(objetivosUsuario);
                _context.Rutinas.RemoveRange(rutinas);
                _context.Objetivos.Remove(objetivo);
                _context.SaveChanges();
            }

            return RedirectToAction("Objetivos");
        }

        public IActionResult Ejercicios(string buscar)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var ejercicios = _context.Ejercicios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                ejercicios = ejercicios.Where(e =>
                    e.Nombre.Contains(buscar) ||
                    e.GrupoMuscular.Contains(buscar) ||
                    e.Categoria.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(ejercicios.OrderByDescending(e => e.Id).ToList());
        }

        [HttpGet]
        public IActionResult NuevoEjercicio()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");
            return View();
        }

        [HttpPost]
        public IActionResult NuevoEjercicio(Ejercicio ejercicio, IFormFile GifArchivo)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            ejercicio.Nombre ??= "";
            ejercicio.GrupoMuscular ??= "";
            ejercicio.Categoria ??= "";
            ejercicio.Descripcion ??= "";
            ejercicio.ComoSeHace ??= "";
            ejercicio.Nivel = string.IsNullOrWhiteSpace(ejercicio.Nivel) ? "Principiante" : ejercicio.Nivel;
            ejercicio.Estado = string.IsNullOrWhiteSpace(ejercicio.Estado) ? "Activo" : ejercicio.Estado;
            ejercicio.Gif = GuardarGif(GifArchivo);

            _context.Ejercicios.Add(ejercicio);
            _context.SaveChanges();

            return RedirectToAction("Ejercicios");
        }

        [HttpGet]
        public IActionResult EditarEjercicio(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var ejercicio = _context.Ejercicios.FirstOrDefault(e => e.Id == id);
            if (ejercicio == null) return RedirectToAction("Ejercicios");

            return View(ejercicio);
        }

        [HttpPost]
        public IActionResult EditarEjercicio(Ejercicio ejercicio, IFormFile GifArchivo)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var ejercicioBD = _context.Ejercicios.FirstOrDefault(e => e.Id == ejercicio.Id);
            if (ejercicioBD == null) return RedirectToAction("Ejercicios");

            ejercicioBD.Nombre = ejercicio.Nombre ?? "";
            ejercicioBD.GrupoMuscular = ejercicio.GrupoMuscular ?? "";
            ejercicioBD.Categoria = ejercicio.Categoria ?? "";
            ejercicioBD.Descripcion = ejercicio.Descripcion ?? "";
            ejercicioBD.ComoSeHace = ejercicio.ComoSeHace ?? "";
            ejercicioBD.Series = ejercicio.Series;
            ejercicioBD.Repeticiones = ejercicio.Repeticiones;
            ejercicioBD.DescansoSegundos = ejercicio.DescansoSegundos;
            ejercicioBD.Nivel = string.IsNullOrWhiteSpace(ejercicio.Nivel) ? "Principiante" : ejercicio.Nivel;
            ejercicioBD.Estado = string.IsNullOrWhiteSpace(ejercicio.Estado) ? "Activo" : ejercicio.Estado;

            var nuevoGif = GuardarGif(GifArchivo);

            if (!string.IsNullOrWhiteSpace(nuevoGif))
                ejercicioBD.Gif = nuevoGif;

            _context.SaveChanges();

            return RedirectToAction("Ejercicios");
        }

        [HttpPost]
        public IActionResult EliminarEjercicio(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var ejercicio = _context.Ejercicios.FirstOrDefault(e => e.Id == id);

            if (ejercicio != null)
            {
                var relaciones = _context.RutinaEjercicios.Where(re => re.EjercicioId == id).ToList();
                var completados = _context.EjerciciosCompletados.Where(ec => ec.EjercicioId == id).ToList();

                _context.RutinaEjercicios.RemoveRange(relaciones);
                _context.EjerciciosCompletados.RemoveRange(completados);

                if (!string.IsNullOrWhiteSpace(ejercicio.Gif))
                {
                    string rutaGif = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "gifs", ejercicio.Gif);

                    if (System.IO.File.Exists(rutaGif))
                        System.IO.File.Delete(rutaGif);
                }

                _context.Ejercicios.Remove(ejercicio);
                _context.SaveChanges();
            }

            return RedirectToAction("Ejercicios");
        }

        public IActionResult Rutinas(string buscar)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var rutinas = _context.Rutinas.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                rutinas = rutinas.Where(r =>
                    r.Nombre.Contains(buscar) ||
                    r.Descripcion.Contains(buscar) ||
                    r.Nivel.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(rutinas.OrderByDescending(r => r.Id).ToList());
        }

        [HttpGet]
        public IActionResult NuevaRutina()
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            ViewBag.Objetivos = _context.Objetivos
                .Where(o => o.Estado == "Activo")
                .OrderBy(o => o.Nombre)
                .ToList();

            return View();
        }

        [HttpPost]
        public IActionResult NuevaRutina(Rutina rutina)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            rutina.Nombre ??= "";
            rutina.Descripcion ??= "";
            rutina.Nivel = string.IsNullOrWhiteSpace(rutina.Nivel) ? "Principiante" : rutina.Nivel;
            rutina.Estado = string.IsNullOrWhiteSpace(rutina.Estado) ? "Activa" : rutina.Estado;
            rutina.FechaCreacion = DateTime.Now;
            rutina.Instrucciones ??= "";

            _context.Rutinas.Add(rutina);
            _context.SaveChanges();

            return RedirectToAction("Rutinas");
        }

        [HttpGet]
        public IActionResult EditarRutina(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var rutina = _context.Rutinas.FirstOrDefault(r => r.Id == id);

            if (rutina == null)
                return RedirectToAction("Rutinas");

            ViewBag.Objetivos = _context.Objetivos
                .Where(o => o.Estado == "Activo")
                .OrderBy(o => o.Nombre)
                .ToList();

            return View(rutina);
        }

        [HttpPost]
        public IActionResult EditarRutina(Rutina rutina)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var rutinaBD = _context.Rutinas.FirstOrDefault(r => r.Id == rutina.Id);

            if (rutinaBD == null)
                return RedirectToAction("Rutinas");

            rutinaBD.ObjetivoId = rutina.ObjetivoId;
            rutinaBD.Nombre = rutina.Nombre ?? "";
            rutinaBD.Descripcion = rutina.Descripcion ?? "";
            rutinaBD.Nivel = string.IsNullOrWhiteSpace(rutina.Nivel) ? "Principiante" : rutina.Nivel;
            rutinaBD.Estado = string.IsNullOrWhiteSpace(rutina.Estado) ? "Activa" : rutina.Estado;
            rutinaBD.Instrucciones = rutina.Instrucciones ?? "";

            _context.SaveChanges();

            return RedirectToAction("Rutinas");
        }

        [HttpPost]
        public IActionResult EliminarRutina(int id)
        {
            if (!EsAdmin()) return RedirectToAction("Login", "Acceso");

            var rutina = _context.Rutinas.FirstOrDefault(r => r.Id == id);

            if (rutina != null)
            {
                var rutinaEjercicios = _context.RutinaEjercicios.Where(re => re.RutinaId == id).ToList();
                var completados = _context.EjerciciosCompletados.Where(ec => ec.RutinaId == id).ToList();

                _context.RutinaEjercicios.RemoveRange(rutinaEjercicios);
                _context.EjerciciosCompletados.RemoveRange(completados);
                _context.Rutinas.Remove(rutina);

                _context.SaveChanges();
            }

            return RedirectToAction("Rutinas");
        }

        private string GuardarGif(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return "";

            string extension = Path.GetExtension(archivo.FileName).ToLower();

            if (extension != ".gif")
                return "";

            string carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "gifs");

            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            string nombreArchivo = Guid.NewGuid().ToString() + extension;
            string rutaArchivo = Path.Combine(carpeta, nombreArchivo);

            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                archivo.CopyTo(stream);
            }

            return nombreArchivo;
        }

        private bool EsAdmin()
        {
            var rol = HttpContext.Session.GetString("RolUsuario");
            return rol != null && rol.Trim().ToLower() == "admin";
        }
    }
}
