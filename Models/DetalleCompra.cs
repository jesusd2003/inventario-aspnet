using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InventarioApp.Models {
    public class DetalleCompra {
        public int Id { get; set; }
        public int CompraId { get; set; }
        public Compra? Compra { get; set; }
        [Required] public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
        [Required][Range(1,int.MaxValue)][Display(Name="Cantidad")] public int Cantidad { get; set; }
        [Column(TypeName="decimal(10,2)")][Display(Name="Costo Unitario")] public decimal CostoUnitario { get; set; }
        [NotMapped] public decimal Subtotal => Cantidad * CostoUnitario;
    }
}