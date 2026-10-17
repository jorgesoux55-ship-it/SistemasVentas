using System.ComponentModel.DataAnnotations;

namespace SistemaVentas.Models;

public class Categoria
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la categoría es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Nombre de la Categoría")]
    public string Nombre { get; set; } = string.Empty;

    public List<Producto> Productos { get; set; } = new();
}