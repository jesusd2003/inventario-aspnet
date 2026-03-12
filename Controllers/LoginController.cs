using InventarioApp.Data; using InventarioApp.Models;
using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace InventarioApp.Controllers {
    public class LoginController : Controller {
        private readonly AppDbContext _db;
        public LoginController(AppDbContext db) { _db = db; }
        public IActionResult Index() {
            if (HttpContext.Session.GetString("UserName") != null) return RedirectToAction("Index","Productos");
            return View();
        }
        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LoginViewModel m) {
            if (!ModelState.IsValid) return View(m);
            var u = await _db.Usuarios.FirstOrDefaultAsync(x=>x.Correo==m.Correo&&x.Password==m.Password);
            if (u == null) { ModelState.AddModelError("","Credenciales incorrectas."); return View(m); }
            HttpContext.Session.SetInt32("UserId",   u.Id);
            HttpContext.Session.SetString("UserName",u.Nombre);
            return RedirectToAction("Index","Productos");
        }
        public IActionResult Logout() { HttpContext.Session.Clear(); return RedirectToAction("Index"); }
    }
}