using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InventarioApp.Models {
    public class Compra {
        public int Id { get; set; }
        [Display(Name="Fecha")] public DateTime Fecha { get; set; } = DateTime.Now;
        [Column(TypeName="decimal(10,2)")][Display(Name="Total")] public decimal Total { get; set; }
        [StringLength(150)][Display(Name="Proveedor")] public string? Proveedor { get; set; }
        [StringLength(300)][Display(Name="Notas")] public string? Notas { get; set; }
        public ICollection<DetalleCompra> Detalles { get; set; } = new List<DetalleCompra>();
    }
}