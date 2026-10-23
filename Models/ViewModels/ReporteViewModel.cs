using SistemaVentas.Models;

namespace SistemaVentas.Models.ViewModels;

public class ReporteFiltroViewModel
{
    public DateTime FechaInicio { get; set; } = DateTime.Today.AddDays(-30);
    public DateTime FechaFin { get; set; } = DateTime.Today;
    public int? ClienteId { get; set; }
    public int? CategoriaId { get; set; }
    public string Periodo { get; set; } = "mes"; // hoy, ayer, semana, mes, mes_anterior, anio, personalizado
}

public class ReporteResumenViewModel
{
    public ReporteFiltroViewModel Filtro { get; set; } = new();

    // Métricas clave del período
    public decimal TotalVendido { get; set; }
    public int TotalTransacciones { get; set; }
    public decimal TicketPromedio { get; set; }
    public int TotalUnidadesVendidas { get; set; }

    // Top productos vendidos
    public List<ProductoVendidoDto> TopProductos { get; set; } = new();

    // Ventas por categoría (para gráfico doughnut)
    public List<CategoriaVentasDto> VentasPorCategoria { get; set; } = new();

    // Ventas por día (para gráfico de tendencia lineal)
    public List<VentaPorDiaDto> VentasPorDia { get; set; } = new();

    // Top clientes compradores
    public List<ClienteVentasDto> TopClientes { get; set; } = new();

    // Métricas de Inventario
    public List<Producto> ProductosStockBajo { get; set; } = new();
    public decimal ValorTotalInventario { get; set; }
    public int TotalProductosAgotados { get; set; }
    public int TotalProductosCatalogo { get; set; }

    // Listado de transacciones
    public List<Venta> Transacciones { get; set; } = new();
}

public class ProductoVendidoDto
{
    public int ProductoId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public int CantidadVendida { get; set; }
    public decimal TotalRecaudado { get; set; }
    public int StockActual { get; set; }
}

public class CategoriaVentasDto
{
    public string Categoria { get; set; } = string.Empty;
    public int CantidadItems { get; set; }
    public decimal TotalMonto { get; set; }
    public double Porcentaje { get; set; }
}

public class VentaPorDiaDto
{
    public string Fecha { get; set; } = string.Empty;
    public string FechaFormateada { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public int CantidadVentas { get; set; }
}

public class ClienteVentasDto
{
    public int ClienteId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public int TotalCompras { get; set; }
    public decimal TotalGastado { get; set; }
}
