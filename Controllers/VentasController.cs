using InventarioApp.Data; using InventarioApp.Models;
using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.Rendering; using Microsoft.EntityFrameworkCore;
namespace InventarioApp.Controllers {
    public class VentasController : Controller {
        private readonly AppDbContext _db;
        public VentasController(AppDbContext db) { _db = db; }
        bool Auth() => HttpContext.Session.GetString("UserName") != null;

        public async Task<IActionResult> Index() {
            if (!Auth()) return RedirectToAction("Index","Login");
            var ventas = await _db.Ventas.Include(v=>v.Cliente).Include(v=>v.Detalles)
                .OrderByDescending(v=>v.Fecha).ToListAsync();
            ViewBag.TotalHoy    = ventas.Where(v=>v.Fecha.Date==DateTime.Today).Sum(v=>v.Total);
            ViewBag.TotalMes    = ventas.Where(v=>v.Fecha.Month==DateTime.Today.Month&&v.Fecha.Year==DateTime.Today.Year).Sum(v=>v.Total);
            ViewBag.TotalGeneral= ventas.Sum(v=>v.Total);
            return View(ventas);
        }

        public async Task<IActionResult> Create() {
            if (!Auth()) return RedirectToAction("Index","Login");
            ViewBag.Clientes  = new SelectList(await _db.Clientes.OrderBy(c=>c.Nombre).ToListAsync(),"Id","Nombre");
            ViewBag.Productos = await _db.Productos.Include(p=>p.Categoria).Where(p=>p.Stock>0).OrderBy(p=>p.Nombre).ToListAsync();
            return View(new VentaCreateVM());
        }

        [HttpPost][ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VentaCreateVM vm) {
            if (!Auth()) return RedirectToAction("Index","Login");
            var items = vm.Items?.Where(i=>i.ProductoId>0&&i.Cantidad>0).ToList();
            if (items==null||!items.Any()) ModelState.AddModelError("","Agrega al menos un producto.");
            if (!ModelState.IsValid) {
                ViewBag.Clientes  = new SelectList(await _db.Clientes.OrderBy(c=>c.Nombre).ToListAsync(),"Id","Nombre",vm.ClienteId);
                ViewBag.Productos = await _db.Productos.Include(p=>p.Categoria).Where(p=>p.Stock>0).OrderBy(p=>p.Nombre).ToListAsync();
                return View(vm);
            }
            // Crear venta
            var venta = new Venta { ClienteId=vm.ClienteId, TipoVenta=vm.TipoVenta, TipoPago=vm.TipoPago, Notas=vm.Notas, Fecha=DateTime.Now };
            _db.Ventas.Add(venta);
            await _db.SaveChangesAsync();
            decimal total = 0;
            foreach (var item in items!) {
                var prod = await _db.Productos.FindAsync(item.ProductoId);
                if (prod==null) continue;
                if (prod.Stock < item.Cantidad) {
                    TempData["Error"]=$"Stock insuficiente para {prod.Nombre}. Disponible: {prod.Stock}";
                    _db.Ventas.Remove(venta); await _db.SaveChangesAsync();
                    ViewBag.Clientes  = new SelectList(await _db.Clientes.OrderBy(c=>c.Nombre).ToListAsync(),"Id","Nombre",vm.ClienteId);
                    ViewBag.Productos = await _db.Productos.Include(p=>p.Categoria).Where(p=>p.Stock>0).OrderBy(p=>p.Nombre).ToListAsync();
                    return View(vm);
                }
                _db.DetallesVenta.Add(new DetalleVenta { VentaId=venta.Id, ProductoId=prod.Id, Cantidad=item.Cantidad, PrecioUnitario=prod.Precio });
                prod.Stock -= item.Cantidad;  // DESCUENTA DEL INVENTARIO
                total += item.Cantidad * prod.Precio;
            }
            venta.Total = total;
            await _db.SaveChangesAsync();
            TempData["Exito"]=$"Venta #{venta.Id} registrada exitosamente por ${total:N2}";
            return RedirectToAction("Details", new{id=venta.Id});
        }

        public async Task<IActionResult> Details(int? id) {
            if (!Auth()) return RedirectToAction("Index","Login");
            if (id==null) return NotFound();
            var v = await _db.Ventas.Include(x=>x.Cliente).Include(x=>x.Detalles).ThenInclude(d=>d.Producto)
                .FirstOrDefaultAsync(x=>x.Id==id);
            return v==null ? NotFound() : View(v);
        }

        public async Task<IActionResult> Reportes() {
            if (!Auth()) return RedirectToAction("Index","Login");
            var hoy   = DateTime.Today;
            var hace7 = hoy.AddDays(-6);
            ViewBag.VentasPorDia = await _db.Ventas.Where(v=>v.Fecha.Date>=hace7)
                .GroupBy(v=>v.Fecha.Date).Select(g=>new{Fecha=g.Key,Total=g.Sum(v=>v.Total),Count=g.Count()}).OrderBy(x=>x.Fecha).ToListAsync();
            ViewBag.TopProductos = await _db.DetallesVenta.Include(d=>d.Producto)
                .GroupBy(d=>new{d.ProductoId,d.Producto!.Nombre})
                .Select(g=>new{g.Key.Nombre,Cantidad=g.Sum(d=>d.Cantidad),Ingresos=g.Sum(d=>d.Cantidad*d.PrecioUnitario)})
                .OrderByDescending(x=>x.Cantidad).Take(5).ToListAsync();
            ViewBag.VentasPorTipo = await _db.Ventas.GroupBy(v=>v.TipoVenta)
                .Select(g=>new{Tipo=g.Key.ToString(),Total=g.Sum(v=>v.Total),Count=g.Count()}).ToListAsync();
            ViewBag.VentasPorPago = await _db.Ventas.GroupBy(v=>v.TipoPago)
                .Select(g=>new{Tipo=g.Key.ToString(),Total=g.Sum(v=>v.Total),Count=g.Count()}).ToListAsync();
            ViewBag.TotalGeneral  = await _db.Ventas.SumAsync(v=>(decimal?)v.Total)??0;
            ViewBag.TotalHoy      = await _db.Ventas.Where(v=>v.Fecha.Date==hoy).SumAsync(v=>(decimal?)v.Total)??0;
            ViewBag.TotalVentas   = await _db.Ventas.CountAsync();
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProducto(int id) {
            var p = await _db.Productos.FindAsync(id);
            if (p==null) return NotFound();
            return Json(new{p.Precio,p.Stock,p.Nombre});
        }
    }
}