using InventarioApp.Data; using InventarioApp.Models;
using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace InventarioApp.Controllers {
    public class CategoriasController : Controller {
        private readonly AppDbContext _db;
        public CategoriasController(AppDbContext db) { _db = db; }
        bool Auth() => HttpContext.Session.GetString("UserName") != null;
        public async Task<IActionResult> Index() {
            if (!Auth()) return RedirectToAction("Index","Login");
            return View(await _db.Categorias.Include(c=>c.Productos).OrderBy(c=>c.Nombre).ToListAsync());
        }
        public IActionResult Create() { if (!Auth()) return RedirectToAction("Index","Login"); return View(); }
        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Categoria c) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (ModelState.IsValid) { _db.Add(c); await _db.SaveChangesAsync(); TempData["Exito"]="Categoria creada."; return RedirectToAction(nameof(Index)); }
            return View(c);
        }
        public async Task<IActionResult> Edit(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var c = await _db.Categorias.FindAsync(id);
            return c==null ? NotFound() : View(c);
        }
        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,[Bind("Id,Nombre")] Categoria c) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id!=c.Id) return NotFound();
            if (ModelState.IsValid) { try { _db.Update(c); await _db.SaveChangesAsync(); TempData["Exito"]="Actualizada."; } catch { if (!_db.Categorias.Any(e=>e.Id==c.Id)) return NotFound(); throw; } return RedirectToAction(nameof(Index)); }
            return View(c);
        }
        public async Task<IActionResult> Delete(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var c = await _db.Categorias.Include(x=>x.Productos).FirstOrDefaultAsync(x=>x.Id==id);
            return c==null ? NotFound() : View(c);
        }
        [HttpPost,ActionName("Delete")][ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            var c = await _db.Categorias.Include(x=>x.Productos).FirstOrDefaultAsync(x=>x.Id==id);
            if (c==null) return NotFound();
            if (c.Productos.Any()) { TempData["Error"]="No se puede eliminar: tiene productos."; return RedirectToAction(nameof(Index)); }
            _db.Remove(c); await _db.SaveChangesAsync(); TempData["Exito"]="Eliminada.";
            return RedirectToAction(nameof(Index));
        }
    }
}