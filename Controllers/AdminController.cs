using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PropositoFit.Data;
using PropositoFit.Models;
using System;
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
            if (!EsAdmin())
            {
                return RedirectToAction("Login", "Acceso");
            }

            return View();
        }

        public IActionResult Usuarios()
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var usuarios = _context.Usuarios.ToList();

            return View(usuarios);
        }

        [HttpGet]
        public IActionResult NuevoUsuario()
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Login", "Acceso");
            }

            return View();
        }

        [HttpPost]
        public IActionResult NuevoUsuario(Usuario usuario)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Login", "Acceso");
            }

            usuario.FechaRegistro = DateTime.Now;

            _context.Usuarios.Add(usuario);
            _context.SaveChanges();

            return RedirectToAction("Usuarios");
        }

        [HttpGet]
        public IActionResult EditarUsuario(int id)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);

            if (usuario == null)
            {
                return RedirectToAction("Usuarios");
            }

            return View(usuario);
        }

        [HttpPost]
        public IActionResult EditarUsuario(Usuario usuario)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var usuarioBD = _context.Usuarios.FirstOrDefault(u => u.Id == usuario.Id);

            if (usuarioBD == null)
            {
                return RedirectToAction("Usuarios");
            }

            usuarioBD.Nombre = usuario.Nombre;
            usuarioBD.Correo = usuario.Correo;
            usuarioBD.Contrasena = usuario.Contrasena;
            usuarioBD.Rol = usuario.Rol;

            _context.SaveChanges();

            return RedirectToAction("Usuarios");
        }

        [HttpPost]
        public IActionResult EliminarUsuario(int id)
        {
            if (!EsAdmin())
            {
                return RedirectToAction("Login", "Acceso");
            }

            var usuario = _context.Usuarios.FirstOrDefault(u => u.Id == id);

            if (usuario != null)
            {
                _context.Database.ExecuteSqlRaw(
                    "DELETE FROM Rutinas WHERE UsuarioId = {0}", id
                );

                _context.Database.ExecuteSqlRaw(
                    "DELETE FROM Objetivos WHERE UsuarioId = {0}", id
                );

                _context.Database.ExecuteSqlRaw(
                    "DELETE FROM Usuarios WHERE Id = {0}", id
                );
            }

            return RedirectToAction("Usuarios");
        }

        private bool EsAdmin()
        {
            var rol = HttpContext.Session.GetString("RolUsuario");
            return rol == "Admin";
        }
    }
}