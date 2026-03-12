using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InventarioApp.Models {
    public class DetalleVenta {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public Venta? Venta { get; set; }
        [Required] public int ProductoId { get; set; }
        public Producto? Producto { get; set; }
        [Required][Range(1,int.MaxValue)][Display(Name="Cantidad")] public int Cantidad { get; set; }
        [Column(TypeName="decimal(10,2)")][Display(Name="Precio")] public decimal PrecioUnitario { get; set; }
        [NotMapped] public decimal Subtotal => Cantidad * PrecioUnitario;
    }
}