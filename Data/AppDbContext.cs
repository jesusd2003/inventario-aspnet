using InventarioApp.Models;
using Microsoft.EntityFrameworkCore;
namespace InventarioApp.Data {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> o) : base(o) {}
        public DbSet<Usuario>      Usuarios      { get; set; }
        public DbSet<Categoria>    Categorias    { get; set; }
        public DbSet<Producto>     Productos     { get; set; }
        public DbSet<Cliente>      Clientes      { get; set; }
        public DbSet<Venta>        Ventas        { get; set; }
        public DbSet<DetalleVenta> DetallesVenta { get; set; }
        public DbSet<Compra>       Compras       { get; set; }
        public DbSet<DetalleCompra> DetallesCompra { get; set; }

        protected override void OnModelCreating(ModelBuilder m) {
            base.OnModelCreating(m);
            m.Entity<Producto>().HasOne(p=>p.Categoria).WithMany(c=>c.Productos).HasForeignKey(p=>p.CategoriaId).OnDelete(DeleteBehavior.Restrict);
            m.Entity<Venta>().HasOne(v=>v.Cliente).WithMany(c=>c.Ventas).HasForeignKey(v=>v.ClienteId).OnDelete(DeleteBehavior.Restrict);
            m.Entity<DetalleVenta>().HasOne(d=>d.Venta).WithMany(v=>v.Detalles).HasForeignKey(d=>d.VentaId).OnDelete(DeleteBehavior.Cascade);
            m.Entity<DetalleVenta>().HasOne(d=>d.Producto).WithMany().HasForeignKey(d=>d.ProductoId).OnDelete(DeleteBehavior.Restrict);
            m.Entity<DetalleCompra>().HasOne(d=>d.Compra).WithMany(c=>c.Detalles).HasForeignKey(d=>d.CompraId).OnDelete(DeleteBehavior.Cascade);
            m.Entity<DetalleCompra>().HasOne(d=>d.Producto).WithMany().HasForeignKey(d=>d.ProductoId).OnDelete(DeleteBehavior.Restrict);
            m.Entity<Usuario>().HasIndex(u=>u.Correo).IsUnique();
            // Seed data
            m.Entity<Usuario>().HasData(new Usuario { Id=1, Nombre="Administrador", Correo="admin@inventario.com", Password="admin123" });
            m.Entity<Categoria>().HasData(new Categoria{Id=1,Nombre="Electronica"}, new Categoria{Id=2,Nombre="Ropa"}, new Categoria{Id=3,Nombre="Alimentos"});
            m.Entity<Cliente>().HasData(
                new Cliente{Id=1,Nombre="Cliente General",Telefono="0000-0000"},
                new Cliente{Id=2,Nombre="Maria Lopez",    Telefono="3001234567"},
                new Cliente{Id=3,Nombre="Carlos Perez",   Telefono="3109876543"});
            m.Entity<Producto>().HasData(
                new Producto{Id=1,Nombre="Laptop Dell",      Precio=12500,Stock=10,CategoriaId=1},
                new Producto{Id=2,Nombre="Teclado Mecanico", Precio=850,  Stock=3, CategoriaId=1},
                new Producto{Id=3,Nombre="Camiseta Basica",  Precio=150,  Stock=4, CategoriaId=2},
                new Producto{Id=4,Nombre="Arroz 5kg",        Precio=85,   Stock=2, CategoriaId=3},
                new Producto{Id=5,Nombre="Aceite Oliva 1L",  Precio=180,  Stock=1, CategoriaId=3});
        }
    }
}