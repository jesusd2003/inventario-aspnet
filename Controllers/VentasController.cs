using InventarioApp.Data; using InventarioApp.Models;
using Microsoft.AspNetCore.Mvc; using Microsoft.AspNetCore.Mvc.Rendering; using Microsoft.EntityFrameworkCore;
namespace InventarioApp.Controllers {
    public class VentasController : Controller {
        private readonly AppDbContext _db;
        public VentasController(AppDbContext db) { _db = db; }
        bool Auth() => HttpContext.Session.GetString("UserName") != null;

        public async Task<IActionResult> Index(string? q, int page = 1) {
            if (!Auth()) return RedirectToAction("Index","Login");
            ViewBag.Q = q;
            var query = _db.Ventas.Include(v=>v.Cliente).Include(v=>v.Detalles).AsQueryable();
            if (!string.IsNullOrWhiteSpace(q)) query = query.Where(v=>v.Cliente!.Nombre.Contains(q) || (v.Notas != null && v.Notas.Contains(q)));
            
            ViewBag.TotalGeneral = await query.SumAsync(v=>(decimal?)v.Total)??0;
            var hoy = DateTime.Today;
            ViewBag.TotalHoy = await query.Where(v=>v.Fecha.Date==hoy).SumAsync(v=>(decimal?)v.Total)??0;
            ViewBag.TotalMes = await query.Where(v=>v.Fecha.Month==hoy.Month && v.Fecha.Year==hoy.Year).SumAsync(v=>(decimal?)v.Total)??0;

            int pageSize = 10;
            var totalItems = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = totalPages;
            ViewBag.CurrentPage = page;
            var lista = await query.OrderByDescending(v=>v.Fecha).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
            return View(lista);
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
            return RedirectToAction("Details", new{id=venta.Id, print=true});
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