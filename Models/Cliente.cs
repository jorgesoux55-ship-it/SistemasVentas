using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models;

public class Cliente
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Apellido")]
    public string? Apellido { get; set; }

    [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
    [StringLength(150)]
    [Display(Name = "Correo Electrónico")]
    public string? Email { get; set; }

    [StringLength(20)]
    [Display(Name = "Teléfono o Celular")]
    public string? Telefono { get; set; }

    public List<Venta> Ventas { get; set; } = new();

    [Display(Name = "Nombre Completo")]
    public string NombreCompleto => string.IsNullOrWhiteSpace(Apellido) 
        ? Nombre 
        : $"{Nombre} {Apellido}".Trim();
}