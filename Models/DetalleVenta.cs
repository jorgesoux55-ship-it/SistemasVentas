using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentas.Models;

public class DetalleVenta
{
    public int Id { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int Cantidad { get; set; } = 1;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    // Relaciones
    public int VentaId { get; set; }
    public Venta? Venta { get; set; }
    // Alias para compatibilidad con código existente
    public Venta? venta { get => Venta; set => Venta = value; }

    [Required]
    public int ProductoId { get; set; }
    public Producto? Producto { get; set; }

    public void CalcularSubtotal()
    {
        Subtotal = Cantidad * Precio;
    }

    // Alias para compatibilidad con DetalleVentasApiController
    public void SubTotal(int cantidad, decimal precio)
    {
        Cantidad = cantidad;
        Precio = precio;
        Subtotal = cantidad * precio;
    }
}