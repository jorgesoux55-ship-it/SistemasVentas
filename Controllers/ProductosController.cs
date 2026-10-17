using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class ProductosController : Controller
{
    private readonly AppDbContext _context;

    public ProductosController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Productos
    public async Task<IActionResult> Index(string? buscar, int? categoriaId)
    {
        var query = _context.Productos.Include(p => p.Categoria).AsQueryable();

        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(p => p.Nombre.ToLower().Contains(buscar.ToLower()));
        }

        if (categoriaId.HasValue && categoriaId.Value > 0)
        {
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        }

        var productos = await query.OrderBy(p => p.Nombre).ToListAsync();
        ViewBag.Categorias = new SelectList(await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre", categoriaId);
        ViewBag.Buscar = buscar;
        return View(productos);
    }

    // GET: /Productos/Create
    public async Task<IActionResult> Create()
    {
        ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre");
        return View(new Producto { Stock = 10, StockMinimo = 5, Precio = 0.00m });
    }

    // POST: /Productos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Producto producto)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Producto '{producto.Nombre}' creado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null) return NotFound();

        ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre", producto.CategoriaId);
        return View(producto);
    }

    // POST: /Productos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Producto producto)
    {
        if (id != producto.Id) return NotFound();

        if (!ModelState.IsValid)
        {
            ViewBag.CategoriaId = new SelectList(await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        _context.Productos.Update(producto);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Producto '{producto.Nombre}' actualizado exitosamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (producto == null) return NotFound();
        return View(producto);
    }

    // POST: /Productos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto != null)
        {
            var tieneVentas = await _context.DetalleVentas.AnyAsync(d => d.ProductoId == id);
            if (tieneVentas)
            {
                TempData["Error"] = "No se puede eliminar: el producto tiene ventas asociadas.";
                return RedirectToAction(nameof(Index));
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Producto eliminado exitosamente.";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Productos/BuscarJson?term=...
    [HttpGet]
    public async Task<IActionResult> BuscarJson(string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return Json(new List<object>());

        var q = term.Trim().ToLower();
        var lista = await _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Nombre.ToLower().Contains(q) || p.Id.ToString() == q)
            .Take(15)
            .Select(p => new
            {
                id = p.Id,
                nombre = p.Nombre,
                precio = p.Precio,
                stock = p.Stock,
                categoria = p.Categoria != null ? p.Categoria.Nombre : ""
            })
            .ToListAsync();

        return Json(lista);
    }
}
