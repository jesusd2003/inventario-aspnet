using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace InventarioApp.Models {
    public enum TipoVenta { POS, Directa }
    public enum TipoPago  { Contado, Credito }
    public class Venta {
        public int Id { get; set; }
        [Display(Name="Fecha")] public DateTime Fecha { get; set; } = DateTime.Now;
        [Column(TypeName="decimal(10,2)")][Display(Name="Total")] public decimal Total { get; set; }
        [Display(Name="Tipo de Venta")] public TipoVenta TipoVenta { get; set; } = TipoVenta.POS;
        [Display(Name="Tipo de Pago")]  public TipoPago  TipoPago  { get; set; } = TipoPago.Contado;
        [StringLength(300)][Display(Name="Notas")] public string? Notas { get; set; }
        [Required][Display(Name="Cliente")] public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }
        public ICollection<DetalleVenta> Detalles { get; set; } = new List<DetalleVenta>();
    }
}