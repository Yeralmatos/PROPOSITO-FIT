using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using PropositoFit.Data;
using PropositoFit.Models;
using PropositoFit.Services;
using System.Linq;

namespace PropositoFit.Controllers
{
    public class AccesoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;

        public AccesoController(ApplicationDbContext context, EmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // LOGIN

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string Correo, string Clave)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == Correo && u.Contrasena == Clave);

            if (usuario != null)
            {
                HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
                HttpContext.Session.SetString("NombreUsuario", usuario.Nombre);
                HttpContext.Session.SetString("RolUsuario", usuario.Rol);

                TempData["NombreUsuario"] = usuario.Nombre;

                if (usuario.Rol == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    return RedirectToAction("Index", "Usuario");
                }
            }

            ViewBag.Error = "Correo o contraseña incorrectos";

            return View();
        }

        // REGISTRO

        [HttpGet]
        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Registro(Usuario usuario)
        {
            usuario.FechaRegistro = DateTime.Now;
            usuario.Rol = "Usuario";

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();
            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);
            HttpContext.Session.SetString("NombreUsuario", usuario.Nombre);

            HttpContext.Session.SetInt32("UsuarioId", usuario.Id);

            TempData["NombreUsuario"] = usuario.Nombre;

            return RedirectToAction("Index", "Usuario");
        }

        // OLVIDÉ MI CONTRASEÑA

        [HttpGet]
        public IActionResult OlvidastePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult OlvidastePassword(string Correo)
        {
            var usuario = _context.Usuarios
                .FirstOrDefault(u => u.Correo == Correo);

            if (usuario == null)
            {
                ViewBag.Error = "No existe una cuenta registrada con ese correo.";
                return View();
            }

            string mensaje = $@"
                <h2>Recuperación de contraseña</h2>
                <p>Hola, {usuario.Nombre}</p>
                <p>Tu contraseña registrada es:</p>
                <h3>{usuario.Contrasena}</h3>
                <p>PropósitoFit</p>
            ";

            _emailService.EnviarCorreo(
                usuario.Correo,
                "Recuperación de contraseña - PropósitoFit",
                mensaje
            );

            ViewBag.Mensaje = "Se envió la información de recuperación a tu correo.";

            return View();
        }

        // CERRAR SESIÓN

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();

            TempData.Clear();

            return RedirectToAction("Login", "Acceso");
        }
    }
}