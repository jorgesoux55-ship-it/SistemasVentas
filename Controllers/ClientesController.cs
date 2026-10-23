using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

[Authorize]
public class ClientesController : Controller
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Clientes
    public async Task<IActionResult> Index(string? buscar)
    {
        var query = _context.Clientes.Include(c => c.Ventas).AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            var q = buscar.ToLower().Trim();
            query = query.Where(c => 
                c.Nombre.ToLower().Contains(q) || 
                (c.Apellido != null && c.Apellido.ToLower().Contains(q)) || 
                (c.Telefono != null && c.Telefono.Contains(q)) ||
                (c.Email != null && c.Email.ToLower().Contains(q)));
        }

        var clientes = await query.OrderBy(c => c.Nombre).ToListAsync();
        ViewBag.Buscar = buscar;
        return View(clientes);
    }

    // GET: /Clientes/Create
    public IActionResult Create()
    {
        return View(new Cliente());
    }

    // POST: /Clientes/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Cliente cliente)
    {
        if (!ModelState.IsValid)
            return View(cliente);

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Cliente '{cliente.NombreCompleto}' registrado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null) return NotFound();

        return View(cliente);
    }

    // POST: /Clientes/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Cliente cliente)
    {
        if (id != cliente.Id) return NotFound();

        if (!ModelState.IsValid)
            return View(cliente);

        _context.Clientes.Update(cliente);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Cliente '{cliente.NombreCompleto}' actualizado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes
            .Include(c => c.Ventas)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cliente == null) return NotFound();
        return View(cliente);
    }

    // POST: /Clientes/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente != null)
        {
            var tieneVentas = await _context.Ventas.AnyAsync(v => v.ClienteId == id);
            if (tieneVentas)
            {
                TempData["Error"] = "No se puede eliminar el cliente porque tiene ventas registradas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cliente eliminado exitosamente.";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Clientes/BuscarJson?term=...
    [HttpGet]
    public async Task<IActionResult> BuscarJson(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Json(new List<object>());

        var q = term.Trim().ToLower();
        var clientes = await _context.Clientes
            .Where(c => c.Nombre.ToLower().Contains(q) || 
                        (c.Apellido != null && c.Apellido.ToLower().Contains(q)) || 
                        (c.Telefono != null && c.Telefono.Contains(q)))
            .Take(10)
            .Select(c => new
            {
                id = c.Id,
                nombre = c.NombreCompleto,
                telefono = c.Telefono ?? "",
                email = c.Email ?? "",
                displayText = string.IsNullOrEmpty(c.Telefono) 
                    ? c.NombreCompleto 
                    : $"{c.NombreCompleto} ({c.Telefono})"
            })
            .ToListAsync();

        return Json(clientes);
    }

    // POST: /Clientes/QuickCreate (para el modal en la vista de venta)
    [HttpPost]
    public async Task<IActionResult> QuickCreate([FromBody] QuickClienteDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            return BadRequest(new { success = false, message = "El nombre es obligatorio" });

        var cliente = new Cliente
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido?.Trim(),
            Telefono = dto.Telefono?.Trim(),
            Email = dto.Email?.Trim()
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        var display = string.IsNullOrEmpty(cliente.Telefono) 
            ? cliente.NombreCompleto 
            : $"{cliente.NombreCompleto} ({cliente.Telefono})";

        return Ok(new
        {
            success = true,
            id = cliente.Id,
            nombre = cliente.NombreCompleto,
            telefono = cliente.Telefono ?? "",
            displayText = display
        });
    }
}

public class QuickClienteDto
{
    public string Nombre { get; set; } = string.Empty;
    public string? Apellido { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
}
