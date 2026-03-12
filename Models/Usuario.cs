using System.ComponentModel.DataAnnotations;
namespace InventarioApp.Models {
    public class Usuario {
        public int Id { get; set; }
        [Required][StringLength(100)][Display(Name="Nombre")] public string Nombre { get; set; } = "";
        [Required][EmailAddress][StringLength(150)][Display(Name="Correo")] public string Correo { get; set; } = "";
        [Required][StringLength(255)][Display(Name="Contrasena")] public string Password { get; set; } = "";
    }
}