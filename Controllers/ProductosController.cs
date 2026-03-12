using InventarioApp.Data; using InventarioApp.Models;
using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.Rendering; using Microsoft.EntityFrameworkCore;
namespace InventarioApp.Controllers {
    public class ProductosController : Controller {
        private readonly AppDbContext _db;
        public ProductosController(AppDbContext db) { _db = db; }
        bool Auth() => HttpContext.Session.GetString("UserName") != null;
        public async Task<IActionResult> Index(string? q) {
            if (!Auth()) return RedirectToAction("Index","Login");
            ViewBag.Q = q;
            var query = _db.Productos.Include(p=>p.Categoria).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(p=>p.Nombre.Contains(q)||(p.Descripcion!=null&&p.Descripcion.Contains(q)));
            var lista = await query.OrderBy(p=>p.Nombre).ToListAsync();
            ViewBag.StockBajo = lista.Count(p=>p.Stock<5);
            return View(lista);
        }
        public async Task<IActionResult> Create() { if (!Auth()) return RedirectToAction("Index","Login"); await Cats(); return View(); }
        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Descripcion,Precio,Stock,CategoriaId")] Producto p) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (ModelState.IsValid) { _db.Add(p); await _db.SaveChangesAsync(); TempData["Exito"]="Producto creado."; return RedirectToAction(nameof(Index)); }
            await Cats(p.CategoriaId); return View(p);
        }
        public async Task<IActionResult> Edit(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var p = await _db.Productos.FindAsync(id);
            if (p==null) return NotFound();
            await Cats(p.CategoriaId); return View(p);
        }
        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,[Bind("Id,Nombre,Descripcion,Precio,Stock,CategoriaId")] Producto p) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id!=p.Id) return NotFound();
            if (ModelState.IsValid) { try { _db.Update(p); await _db.SaveChangesAsync(); TempData["Exito"]="Actualizado."; } catch { if (!_db.Productos.Any(e=>e.Id==p.Id)) return NotFound(); throw; } return RedirectToAction(nameof(Index)); }
            await Cats(p.CategoriaId); return View(p);
        }
        public async Task<IActionResult> Delete(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var p = await _db.Productos.Include(x=>x.Categoria).FirstOrDefaultAsync(x=>x.Id==id);
            return p==null ? NotFound() : View(p);
        }
        [HttpPost,ActionName("Delete")][ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            var p = await _db.Productos.FindAsync(id);
            if (p==null) return NotFound();
            _db.Remove(p); await _db.SaveChangesAsync(); TempData["Exito"]="Eliminado.";
            return RedirectToAction(nameof(Index));
        }
        async Task Cats(int? sel=null) =>
            ViewBag.CategoriaId = new SelectList(await _db.Categorias.OrderBy(c=>c.Nombre).ToListAsync(),"Id","Nombre",sel);
    }
}