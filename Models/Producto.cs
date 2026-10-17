using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentas.Models;

public class Producto
{
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(150)]
    [Display(Name = "Nombre del Producto")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Precio (Bs)")]
    public decimal Precio { get; set; } = 0.00m;

    [Required(ErrorMessage = "El stock es obligatorio")]
    [Range(0, 100000, ErrorMessage = "El stock debe ser un valor no negativo")]
    [Display(Name = "Stock Actual")]
    public int Stock { get; set; } = 0;

    [Display(Name = "Stock Mínimo")]
    [Range(0, 10000, ErrorMessage = "El stock mínimo no puede ser negativo")]
    public int StockMinimo { get; set; } = 5;

    [Required(ErrorMessage = "Debe seleccionar una categoría")]
    [Display(Name = "Categoría")]
    public int CategoriaId { get; set; }

    public Categoria? Categoria { get; set; }

    public bool TieneStock() => Stock > 0;
    public bool StockBajo() => Stock <= StockMinimo;
}