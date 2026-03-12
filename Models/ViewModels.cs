using System.ComponentModel.DataAnnotations;
namespace InventarioApp.Models {
    public class LoginViewModel {
        [Required(ErrorMessage="El correo es obligatorio")][EmailAddress][Display(Name="Correo")] public string Correo { get; set; } = "";
        [Required(ErrorMessage="La contrasena es obligatoria")][DataType(DataType.Password)][Display(Name="Contrasena")] public string Password { get; set; } = "";
    }
    public class ItemLineaInput {
        public int ProductoId  { get; set; }
        public int Cantidad    { get; set; }
    }
    public class VentaCreateVM {
        [Required(ErrorMessage="El cliente es obligatorio")][Display(Name="Cliente")] public int ClienteId { get; set; }
        [Display(Name="Tipo de Venta")] public TipoVenta TipoVenta { get; set; } = TipoVenta.POS;
        [Display(Name="Tipo de Pago")]  public TipoPago  TipoPago  { get; set; } = TipoPago.Contado;
        [StringLength(300)][Display(Name="Notas")] public string? Notas { get; set; }
        public List<ItemLineaInput> Items { get; set; } = new();
    }
    public class CompraCreateVM {
        [StringLength(150)][Display(Name="Proveedor")] public string? Proveedor { get; set; }
        [StringLength(300)][Display(Name="Notas")]     public string? Notas     { get; set; }
        public List<ItemLineaInput> Items { get; set; } = new();
    }
}