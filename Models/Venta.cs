using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SistemaVentas.Models;

public class Venta
{
    public int Id { get; set; }

    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Now;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Total (Bs)")]
    public decimal Total { get; set; } = 0.00m;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Recibido (Bs)")]
    public decimal Recibido { get; set; } = 0.00m;

    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Cambio (Bs)")]
    public decimal Cambio { get; set; } = 0.00m;

    [Required(ErrorMessage = "Debe seleccionar un cliente")]
    [Display(Name = "Cliente")]
    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    public List<DetalleVenta> Detalles { get; set; } = new();

    // Métodos de negocio
    public void CalcularTotal()
    {
        Total = Detalles.Sum(d => d.Subtotal);
    }

    public void CalcularCambio()
    {
        Cambio = Recibido >= Total ? Recibido - Total : 0.00m;
    }

    // Alias para compatibilidad con código existente
    public void CalcularVambio() => CalcularCambio();

    public void ActualizarStock()
    {
        foreach (var d in Detalles)
        {
            if (d.Producto != null)
            {
                d.Producto.Stock -= d.Cantidad;
            }
        }
    }

    // Alias para compatibilidad con código existente
    public void Actualizarstock() => ActualizarStock();
}
