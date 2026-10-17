using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;

namespace SistemaVentas.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var hoy = DateTime.Today;

        ViewBag.TotalProductos = await _context.Productos.CountAsync();
        ViewBag.TotalClientes = await _context.Clientes.CountAsync();
        ViewBag.TotalCategorias = await _context.Categorias.CountAsync();

        var ventasHoy = await _context.Ventas
            .Where(v => v.Fecha >= hoy)
            .ToListAsync();

        ViewBag.VentasHoyCantidad = ventasHoy.Count;
        ViewBag.VentasHoyMonto = ventasHoy.Sum(v => v.Total);

        var ventasHistorico = await _context.Ventas.ToListAsync();
        ViewBag.TotalVentasHistorico = ventasHistorico.Count;
        ViewBag.TotalRecaudadoHistorico = ventasHistorico.Sum(v => v.Total);

        ViewBag.ProductosStockBajo = await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Stock <= p.StockMinimo)
            .OrderBy(p => p.Stock)
            .Take(5)
            .ToListAsync();

        ViewBag.UltimasVentas = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
            .OrderByDescending(v => v.Fecha)
            .Take(5)
            .ToListAsync();

        return View();
    }

    public IActionResult Error()
    {
        return View();
    }
}
