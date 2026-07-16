using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using PropositoFit.Data;
using PropositoFit.Models;
using PropositoFit.Services;
using System;
using System.Linq;

namespace PropositoFit.Controllers
{
    public class AccesoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        public AccesoController(
     ApplicationDbContext context,
     EmailService emailService,
     IConfiguration configuration)
        {
            _context = context;
            _emailService = emailService;
            _configuration = configuration;
        }

        private async Task<bool> ValidarCaptcha(string token)
        {
            var secret = _configuration["GoogleReCaptcha:SecretKey"];

            using var cliente = new HttpClient();

            var respuesta = await cliente.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={token}",
                null);

            var json = await respuesta.Content.ReadAsStringAsync();

            using var documento = JsonDocument.Parse(json);

            return documento.RootElement.GetProperty("success").GetBoolean();
        }

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
                HttpContext.Session.SetString("NombreUsuario", usuario.NombreCliente ?? "");
                HttpContext.Session.SetString("RolUsuario", usuario.Rol ?? "Cliente");

                TempData["NombreUsuario"] = usuario.NombreCliente;

                if (usuario.Rol != null &&
                    usuario.Rol.Trim().ToLower() == "admin")
                {
                    return RedirectToAction("Index", "Admin");
                }

                return RedirectToAction("Index", "Usuario");
            }

            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        [HttpGet]
        public IActionResult Registro()
        {
            ViewBag.SiteKey = _configuration["GoogleReCaptcha:SiteKey"];
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(Usuario usuario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.SiteKey = _configuration["GoogleReCaptcha:SiteKey"];
                return View(usuario);
            }

            // ===========================
            // VALIDAR CAPTCHA
            // ===========================

            var captchaToken = Request.Form["g-recaptcha-response"];

            if (string.IsNullOrWhiteSpace(captchaToken))
            {
                ViewBag.SiteKey = _configuration["GoogleReCaptcha:SiteKey"];
                ModelState.AddModelError("", "Debe confirmar que no es un robot.");
                return View(usuario);
            }

            bool captchaValido = await ValidarCaptcha(captchaToken);

            if (!captchaValido)
            {
                ViewBag.SiteKey = _configuration["GoogleReCaptcha:SiteKey"];
                ModelState.AddModelError("", "La validación del reCAPTCHA falló.");
                return View(usuario);
            }

            // ===========================
            // VALIDAR NOMBRE DE USUARIO
            // ===========================

            bool existeUsuario = _context.Usuarios.Any(u =>
                u.NombreUsuario == usuario.NombreUsuario);

            if (existeUsuario)
            {
                ViewBag.SiteKey = _configuration["GoogleReCaptcha:SiteKey"];

                ModelState.AddModelError(
                    "NombreUsuario",
                    "El nombre de usuario ya está registrado.");

                return View(usuario);
            }

            // ===========================
            // VALIDAR CORREO
            // ===========================

            bool existeCorreo = _context.Usuarios.Any(u =>
                u.Correo == usuario.Correo);

            if (existeCorreo)
            {
                ViewBag.SiteKey = _configuration["GoogleReCaptcha:SiteKey"];

                ModelState.AddModelError(
                    "Correo",
                    "El correo ya está registrado.");

                return View(usuario);
            }

            // ===========================
            // GUARDAR
            // ===========================

            usuario.FechaRegistro = DateTime.Now;
            usuario.Estado = "Activo";
            usuario.Rol = "Cliente";

            _context.Usuarios.Add(usuario);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Registro realizado correctamente. Inicie sesión.";

            return RedirectToAction("Login");
        }
        //====================================
        // RECUPERAR CONTRASEÑA
        //====================================

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

                <p>Hola <b>{usuario.NombreCliente}</b>.</p>

                <p>Tu contraseña registrada es:</p>

                <h3>{usuario.Contrasena}</h3>

                <br>

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

        //====================================
        // CERRAR SESIÓN
        //====================================

        public IActionResult CerrarSesion()
        {
            HttpContext.Session.Clear();
            TempData.Clear();

            return RedirectToAction("Login");
        }
    }
}