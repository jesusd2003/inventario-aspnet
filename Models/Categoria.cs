using System.ComponentModel.DataAnnotations;
namespace InventarioApp.Models {
    public class Categoria {
        public int Id { get; set; }
        [Required(ErrorMessage="El nombre es obligatorio")][StringLength(100)][Display(Name="Nombre")] public string Nombre { get; set; } = "";
        public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    }
}