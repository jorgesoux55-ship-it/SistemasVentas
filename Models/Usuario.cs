using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentas.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [Display(Name = "Nombre Completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El usuario debe tener entre 3 y 50 caracteres")]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe asignar un rol")]
    [StringLength(30)]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = "Vendedor"; // "Administrador", "Vendedor", "Cajero"

    [Display(Name = "Estado")]
    public bool Activo { get; set; } = true;

    [Display(Name = "Fecha de Registro")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Último Acceso")]
    public DateTime? UltimoAcceso { get; set; }
}
