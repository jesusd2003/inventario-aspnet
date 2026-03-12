using System.ComponentModel.DataAnnotations;
namespace InventarioApp.Models {
    public class Cliente {
        public int Id { get; set; }
        [Required(ErrorMessage="El nombre es obligatorio")][StringLength(150)][Display(Name="Nombre")] public string Nombre { get; set; } = "";
        [StringLength(100)][Display(Name="Telefono")] public string? Telefono { get; set; }
        [StringLength(200)][Display(Name="Direccion")] public string? Direccion { get; set; }
        public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
        // Total comprado por este cliente
        public decimal TotalComprado => Ventas.Sum(v => v.Total);
    }
}