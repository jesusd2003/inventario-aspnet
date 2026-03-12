using InventarioApp.Data; using InventarioApp.Models;
using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore;
namespace InventarioApp.Controllers {
    public class ComprasController : Controller {
        private readonly AppDbContext _db;
        public ComprasController(AppDbContext db) { _db = db; }
        bool Auth() => HttpContext.Session.GetString("UserName") != null;

        public async Task<IActionResult> Index() {
            if (!Auth()) return RedirectToAction("Index","Login");
            var compras = await _db.Compras.Include(c=>c.Detalles).OrderByDescending(c=>c.Fecha).ToListAsync();
            return View(compras);
        }

        public async Task<IActionResult> Create() {
            if (!Auth()) return RedirectToAction("Index","Login");
            ViewBag.Productos = await _db.Productos.Include(p=>p.Categoria).OrderBy(p=>p.Nombre).ToListAsync();
            return View(new CompraCreateVM());
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompraCreateVM vm) {
            if (!Auth()) return RedirectToAction("Index","Login");
            var items = vm.Items?.Where(i=>i.ProductoId>0&&i.Cantidad>0).ToList();
            if (items==null||!items.Any()) ModelState.AddModelError("","Agrega al menos un producto.");
            if (!ModelState.IsValid) {
                ViewBag.Productos = await _db.Productos.Include(p=>p.Categoria).OrderBy(p=>p.Nombre).ToListAsync();
                return View(vm);
            }
            var compra = new Compra { Proveedor=vm.Proveedor, Notas=vm.Notas, Fecha=DateTime.Now };
            _db.Compras.Add(compra);
            await _db.SaveChangesAsync();
            decimal total = 0;
            foreach (var item in items!) {
                var prod = await _db.Productos.FindAsync(item.ProductoId);
                if (prod==null) continue;
                var costo = prod.Precio * 0.6m; // Costo estimado 60% del precio venta
                _db.DetallesCompra.Add(new DetalleCompra { CompraId=compra.Id, ProductoId=prod.Id, Cantidad=item.Cantidad, CostoUnitario=costo });
                prod.Stock += item.Cantidad;  // SUMA AL INVENTARIO
                total += item.Cantidad * costo;
            }
            compra.Total = total;
            await _db.SaveChangesAsync();
            TempData["Exito"]=$"Compra #{compra.Id} registrada. Stock actualizado.";
            return RedirectToAction("Details", new{id=compra.Id});
        }

        public async Task<IActionResult> Details(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var c = await _db.Compras.Include(x=>x.Detalles).ThenInclude(d=>d.Producto)
                .FirstOrDefaultAsync(x=>x.Id==id);
            return c==null ? NotFound() : View(c);
        }
    }
}