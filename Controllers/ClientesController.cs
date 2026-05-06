using InventarioApp.Data; using InventarioApp.Models;
using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace InventarioApp.Controllers {
    public class ClientesController : Controller {
        private readonly AppDbContext _db;
        public ClientesController(AppDbContext db) { _db = db; }
        bool Auth() => HttpContext.Session.GetString("UserName") != null;

        // Lista de clientes con total comprado
        public async Task<IActionResult> Index(string? q, int page = 1) {
            if (!Auth()) return RedirectToAction("Index","Login");
            ViewBag.Q = q;
            var query = _db.Clientes.Include(c=>c.Ventas).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(c=>c.Nombre.Contains(q) || (c.Telefono != null && c.Telefono.Contains(q)) || (c.Direccion != null && c.Direccion.Contains(q)));
            int pageSize = 10;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            var lista = await query.OrderBy(c=>c.Nombre).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(lista);
        }

        // Detalle de un cliente: sus ventas
        public async Task<IActionResult> Details(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var c = await _db.Clientes.Include(x=>x.Ventas).ThenInclude(v=>v.Detalles).ThenInclude(d=>d.Producto)
                .FirstOrDefaultAsync(x=>x.Id==id);
            return c==null ? NotFound() : View(c);
        }

        public IActionResult Create() { if (!Auth()) return RedirectToAction("Index","Login"); return View(); }
        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre,Telefono,Direccion")] Cliente c) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (ModelState.IsValid) { _db.Add(c); await _db.SaveChangesAsync(); TempData["Exito"]="Cliente creado."; return RedirectToAction(nameof(Index)); }
            return View(c);
        }
        public async Task<IActionResult> Edit(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var c = await _db.Clientes.FindAsync(id);
            return c==null ? NotFound() : View(c);
        }
        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,[Bind("Id,Nombre,Telefono,Direccion")] Cliente c) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id!=c.Id) return NotFound();
            if (ModelState.IsValid) { try { _db.Update(c); await _db.SaveChangesAsync(); TempData["Exito"]="Actualizado."; } catch { if (!_db.Clientes.Any(e=>e.Id==c.Id)) return NotFound(); throw; } return RedirectToAction(nameof(Index)); }
            return View(c);
        }
        public async Task<IActionResult> Delete(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var c = await _db.Clientes.Include(x=>x.Ventas).FirstOrDefaultAsync(x=>x.Id==id);
            return c==null ? NotFound() : View(c);
        }
        [HttpPost,ActionName("Delete")][ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            var c = await _db.Clientes.Include(x=>x.Ventas).FirstOrDefaultAsync(x=>x.Id==id);
            if (c==null) return NotFound();
            if (c.Ventas.Any()) { TempData["Error"]="No se puede eliminar: tiene ventas registradas."; return RedirectToAction(nameof(Index)); }
            _db.Remove(c); await _db.SaveChangesAsync(); TempData["Exito"]="Cliente eliminado.";
            return RedirectToAction(nameof(Index));
        }

        // API para buscar clientes (autocompletado)
        [HttpGet]
        public async Task<IActionResult> Buscar(string q) {
            var clientes = await _db.Clientes.Where(c=>c.Nombre.Contains(q)).Take(10)
                .Select(c=>new{c.Id,c.Nombre,c.Telefono}).ToListAsync();
            return Json(clientes);
        }
    }
}