using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

[Authorize]
public class VentasController : Controller
{
    private readonly AppDbContext _context;

    public VentasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Ventas
    public async Task<IActionResult> Index(string? buscar, DateTime? fecha)
    {
        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var q = buscar.ToLower().Trim();
            query = query.Where(v => 
                (v.Cliente != null && (v.Cliente.Nombre.ToLower().Contains(q) || (v.Cliente.Apellido != null && v.Cliente.Apellido.ToLower().Contains(q)))) ||
                v.Id.ToString() == q);
        }

        if (fecha.HasValue)
        {
            var f = fecha.Value.Date;
            query = query.Where(v => v.Fecha.Date == f);
        }

        var ventas = await query.OrderByDescending(v => v.Fecha).ToListAsync();

        ViewBag.Buscar = buscar;
        ViewBag.Fecha = fecha?.ToString("yyyy-MM-dd");
        ViewBag.TotalRecaudado = ventas.Sum(v => v.Total);
        ViewBag.CantidadVentas = ventas.Count;

        return View(ventas);
    }

    // GET: /Ventas/Create
    public async Task<IActionResult> Create()
    {
        // Clientes y productos iniciales para acelerar la carga en la vista
        ViewBag.Clientes = await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Productos = await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Stock > 0)
            .OrderBy(p => p.Nombre)
            .ToListAsync();

        // Cliente por defecto si existe (por ejemplo Andy o el primero)
        var clienteAndy = await _context.Clientes.FirstOrDefaultAsync(c => c.Nombre.Contains("Andy")) 
                          ?? await _context.Clientes.FirstOrDefaultAsync();

        ViewBag.ClienteInicial = clienteAndy;

        return View();
    }

    // POST: /Ventas/Create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] RegistrarVentaDto model)
    {
        if (model == null)
            return BadRequest(new { success = false, message = "Datos de venta no recibidos." });

        if (model.ClienteId <= 0)
            return BadRequest(new { success = false, message = "Debe seleccionar un cliente para la venta." });

        if (model.Detalles == null || !model.Detalles.Any())
            return BadRequest(new { success = false, message = "Debe agregar al menos un producto a la venta." });

        // Validar cliente
        var cliente = await _context.Clientes.FindAsync(model.ClienteId);
        if (cliente == null)
            return BadRequest(new { success = false, message = "El cliente seleccionado no existe." });

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var venta = new Venta
            {
                ClienteId = model.ClienteId,
                Fecha = model.Fecha != default ? model.Fecha : DateTime.Now,
                Recibido = model.Recibido,
                Detalles = new List<DetalleVenta>()
            };

            decimal totalCalculado = 0;

            foreach (var item in model.Detalles)
            {
                if (item.Cantidad <= 0)
                    return BadRequest(new { success = false, message = $"Cantidad inválida para el producto ID {item.ProductoId}." });

                var producto = await _context.Productos.FindAsync(item.ProductoId);
                if (producto == null)
                    return BadRequest(new { success = false, message = $"El producto con ID {item.ProductoId} no existe." });

                if (producto.Stock < item.Cantidad)
                {
                    return BadRequest(new { 
                        success = false, 
                        message = $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Stock}, Solicitado: {item.Cantidad}." 
                    });
                }

                // Descontar stock
                producto.Stock -= item.Cantidad;

                var subtotal = item.Cantidad * producto.Precio;
                totalCalculado += subtotal;

                venta.Detalles.Add(new DetalleVenta
                {
                    ProductoId = producto.Id,
                    Cantidad = item.Cantidad,
                    Precio = producto.Precio,
                    Subtotal = subtotal
                });
            }

            venta.Total = totalCalculado;
            venta.CalcularCambio();

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                success = true,
                id = venta.Id,
                total = venta.Total,
                cambio = venta.Cambio,
                message = "¡Venta registrada con éxito!"
            });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { success = false, message = "Error al procesar la venta: " + ex.Message });
        }
    }

    // GET: /Ventas/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
                    .ThenInclude(p => p!.Categoria)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();

        return View(venta);
    }

    // POST: /Ventas/Anular/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Anular(int id)
    {
        var venta = await _context.Ventas
            .Include(v => v.Detalles)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (venta == null) return NotFound();

        // Restaurar stock
        foreach (var det in venta.Detalles)
        {
            if (det.Producto != null)
            {
                det.Producto.Stock += det.Cantidad;
            }
        }

        _context.Ventas.Remove(venta);
        await _context.SaveChangesAsync();

        TempData["Success"] = $"Venta #{id} anulada y stock restaurado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}

public class RegistrarVentaDto
{
    public int ClienteId { get; set; }
    public DateTime Fecha { get; set; } = DateTime.Now;
    public decimal Recibido { get; set; }
    public List<DetalleItemDto> Detalles { get; set; } = new();
}

public class DetalleItemDto
{
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
}