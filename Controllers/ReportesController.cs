using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;
using SistemaVentas.Models.ViewModels;

namespace SistemaVentas.Controllers;

[Authorize]
public class ReportesController : Controller
{
    private readonly AppDbContext _context;

    public ReportesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        string periodo = "mes",
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        int? clienteId = null,
        int? categoriaId = null)
    {
        // 1) Determinar rango de fechas según período
        DateTime inicio;
        DateTime fin;
        var ahora = DateTime.Now;
        var hoy = DateTime.Today;

        switch (periodo?.ToLower())
        {
            case "hoy":
                inicio = hoy;
                fin = hoy.AddDays(1).AddTicks(-1);
                break;
            case "ayer":
                inicio = hoy.AddDays(-1);
                fin = hoy.AddTicks(-1);
                break;
            case "semana":
                // Últimos 7 días
                inicio = hoy.AddDays(-6);
                fin = hoy.AddDays(1).AddTicks(-1);
                break;
            case "mes_anterior":
                var primerDiaMesActual = new DateTime(hoy.Year, hoy.Month, 1);
                fin = primerDiaMesActual.AddTicks(-1);
                inicio = new DateTime(fin.Year, fin.Month, 1);
                break;
            case "anio":
                inicio = new DateTime(hoy.Year, 1, 1);
                fin = new DateTime(hoy.Year, 12, 31, 23, 59, 59);
                break;
            case "personalizado":
                inicio = fechaInicio.HasValue ? fechaInicio.Value.Date : hoy.AddDays(-30);
                fin = fechaFin.HasValue ? fechaFin.Value.Date.AddDays(1).AddTicks(-1) : hoy.AddDays(1).AddTicks(-1);
                break;
            case "mes":
            default:
                periodo = "mes";
                inicio = new DateTime(hoy.Year, hoy.Month, 1);
                fin = hoy.AddDays(1).AddTicks(-1);
                break;
        }

        // 2) Consulta base de ventas con relaciones
        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p!.Categoria)
            .Where(v => v.Fecha >= inicio && v.Fecha <= fin);

        if (clienteId.HasValue && clienteId.Value > 0)
        {
            query = query.Where(v => v.ClienteId == clienteId.Value);
        }

        var ventasPeriodo = await query.OrderByDescending(v => v.Fecha).ToListAsync();

        // 3) Construir modelo de resumen
        var viewModel = new ReporteResumenViewModel
        {
            Filtro = new ReporteFiltroViewModel
            {
                Periodo = periodo,
                FechaInicio = inicio,
                FechaFin = fin,
                ClienteId = clienteId,
                CategoriaId = categoriaId
            },
            TotalVendido = ventasPeriodo.Sum(v => v.Total),
            TotalTransacciones = ventasPeriodo.Count,
            Transacciones = ventasPeriodo
        };

        viewModel.TicketPromedio = viewModel.TotalTransacciones > 0
            ? Math.Round(viewModel.TotalVendido / viewModel.TotalTransacciones, 2)
            : 0;

        // Aplanar todos los detalles del periodo
        var todosDetalles = ventasPeriodo.SelectMany(v => v.Detalles).ToList();
        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            todosDetalles = todosDetalles
                .Where(d => d.Producto?.CategoriaId == categoriaId.Value)
                .ToList();
        }

        viewModel.TotalUnidadesVendidas = todosDetalles.Sum(d => d.Cantidad);

        // Top Productos
        viewModel.TopProductos = todosDetalles
            .Where(d => d.Producto != null)
            .GroupBy(d => new { d.ProductoId, d.Producto!.Nombre, CategoriaNombre = d.Producto.Categoria?.Nombre ?? "Sin categoría", d.Producto.Stock })
            .Select(g => new ProductoVendidoDto
            {
                ProductoId = g.Key.ProductoId,
                Nombre = g.Key.Nombre,
                Categoria = g.Key.CategoriaNombre,
                CantidadVendida = g.Sum(x => x.Cantidad),
                TotalRecaudado = g.Sum(x => x.Subtotal),
                StockActual = g.Key.Stock
            })
            .OrderByDescending(p => p.TotalRecaudado)
            .Take(10)
            .ToList();

        // Ventas por Categoría (para gráfico de dona)
        var totalDetallesMonto = todosDetalles.Sum(d => d.Subtotal);
        viewModel.VentasPorCategoria = todosDetalles
            .Where(d => d.Producto?.Categoria != null)
            .GroupBy(d => d.Producto!.Categoria!.Nombre)
            .Select(g => new CategoriaVentasDto
            {
                Categoria = g.Key,
                CantidadItems = g.Sum(x => x.Cantidad),
                TotalMonto = g.Sum(x => x.Subtotal),
                Porcentaje = totalDetallesMonto > 0 ? (double)Math.Round((g.Sum(x => x.Subtotal) / totalDetallesMonto) * 100, 1) : 0
            })
            .OrderByDescending(c => c.TotalMonto)
            .ToList();

        // Ventas por Día (para gráfico cronológico)
        viewModel.VentasPorDia = ventasPeriodo
            .GroupBy(v => v.Fecha.Date)
            .OrderBy(g => g.Key)
            .Select(g => new VentaPorDiaDto
            {
                Fecha = g.Key.ToString("yyyy-MM-dd"),
                FechaFormateada = g.Key.ToString("dd/MM"),
                Total = g.Sum(x => x.Total),
                CantidadVentas = g.Count()
            })
            .ToList();

        // Top Clientes
        viewModel.TopClientes = ventasPeriodo
            .Where(v => v.Cliente != null)
            .GroupBy(v => new { v.ClienteId, v.Cliente!.NombreCompleto, v.Cliente.Telefono })
            .Select(g => new ClienteVentasDto
            {
                ClienteId = g.Key.ClienteId,
                NombreCompleto = g.Key.NombreCompleto,
                Telefono = g.Key.Telefono ?? "-",
                TotalCompras = g.Count(),
                TotalGastado = g.Sum(x => x.Total)
            })
            .OrderByDescending(c => c.TotalGastado)
            .Take(5)
            .ToList();

        // Métricas de Inventario
        var todosProductos = await _context.Productos.Include(p => p.Categoria).ToListAsync();
        viewModel.TotalProductosCatalogo = todosProductos.Count;
        viewModel.TotalProductosAgotados = todosProductos.Count(p => p.Stock <= 0);
        viewModel.ValorTotalInventario = todosProductos.Sum(p => p.Precio * p.Stock);
        viewModel.ProductosStockBajo = todosProductos
            .Where(p => p.Stock <= p.StockMinimo)
            .OrderBy(p => p.Stock)
            .ToList();

        // Cargar listas para filtros de selección
        ViewBag.Clientes = new SelectList(await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync(), "Id", "NombreCompleto", clienteId);
        ViewBag.Categorias = new SelectList(await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre", categoriaId);

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> ExportarCsv(
        string periodo = "mes",
        DateTime? fechaInicio = null,
        DateTime? fechaFin = null,
        int? clienteId = null)
    {
        DateTime inicio;
        DateTime fin;
        var hoy = DateTime.Today;

        switch (periodo?.ToLower())
        {
            case "hoy":
                inicio = hoy;
                fin = hoy.AddDays(1).AddTicks(-1);
                break;
            case "ayer":
                inicio = hoy.AddDays(-1);
                fin = hoy.AddTicks(-1);
                break;
            case "semana":
                inicio = hoy.AddDays(-6);
                fin = hoy.AddDays(1).AddTicks(-1);
                break;
            case "personalizado":
                inicio = fechaInicio.HasValue ? fechaInicio.Value.Date : hoy.AddDays(-30);
                fin = fechaFin.HasValue ? fechaFin.Value.Date.AddDays(1).AddTicks(-1) : hoy.AddDays(1).AddTicks(-1);
                break;
            case "mes":
            default:
                inicio = new DateTime(hoy.Year, hoy.Month, 1);
                fin = hoy.AddDays(1).AddTicks(-1);
                break;
        }

        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .Where(v => v.Fecha >= inicio && v.Fecha <= fin);

        if (clienteId.HasValue && clienteId.Value > 0)
        {
            query = query.Where(v => v.ClienteId == clienteId.Value);
        }

        var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();

        var sb = new StringBuilder();
        // Encabezados con separador de punto y coma para Excel
        sb.AppendLine("ID Venta;Fecha;Hora;Cliente;Telefono;Total (Bs);Recibido (Bs);Cambio (Bs);Items Vendidos");

        foreach (var v in ventas)
        {
            var cliente = v.Cliente != null ? v.Cliente.NombreCompleto.Replace(";", " ") : "Mostrador General";
            var tel = v.Cliente?.Telefono ?? "-";
            var itemsCount = v.Detalles.Sum(d => d.Cantidad);

            sb.AppendLine($"{v.Id};{v.Fecha:dd/MM/yyyy};{v.Fecha:HH:mm};\"{cliente}\";\"{tel}\";{v.Total.ToString("F2", CultureInfo.InvariantCulture)};{v.Recibido.ToString("F2", CultureInfo.InvariantCulture)};{v.Cambio.ToString("F2", CultureInfo.InvariantCulture)};{itemsCount}");
        }

        var fileName = $"Reporte_Ventas_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

        return File(bytes, "text/csv; charset=utf-8", fileName);
    }
}
