using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InventarioApp.Models {
    public class Producto {
        public int Id { get; set; }
        [Required][StringLength(150)][Display(Name="Nombre")] public string Nombre { get; set; } = "";
        [StringLength(500)][Display(Name="Descripcion")] public string? Descripcion { get; set; }
        [Required][Range(0.01,999999.99)][Column(TypeName="decimal(10,2)")][Display(Name="Precio Venta")] public decimal Precio { get; set; }
        [Required][Range(0,int.MaxValue)][Display(Name="Stock")] public int Stock { get; set; }
        [Required][Display(Name="Categoria")] public int CategoriaId { get; set; }
        public Categoria? Categoria { get; set; }
        [NotMapped] public bool StockBajo => Stock < 5;
    }
}