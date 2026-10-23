using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Por favor ingrese su nombre de usuario")]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Por favor ingrese su contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Mantener sesión iniciada")]
    public bool Recordarme { get; set; } = false;

    public string? ReturnUrl { get; set; }
}
