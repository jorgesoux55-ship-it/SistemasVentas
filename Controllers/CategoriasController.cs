using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Data;
using SistemaVentas.Models;

namespace SistemaVentas.Controllers;

public class CategoriasController : Controller
{
    private readonly AppDbContext _context;

    public CategoriasController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Categorias
    public async Task<IActionResult> Index()
    {
        var categorias = await _context.Categorias
            .Include(c => c.Productos)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
        return View(categorias);
    }

    // GET: /Categorias/Create
    public IActionResult Create()
    {
        return View(new Categoria());
    }

    // POST: /Categorias/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Categoria categoria)
    {
        if (!ModelState.IsValid)
            return View(categoria);

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Categoría '{categoria.Nombre}' creada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Categorias/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null) 
            return NotFound();

        return View(categoria);
    }

    // POST: /Categorias/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Categoria categoria)
    {
        if (id != categoria.Id) 
            return NotFound();

        if (!ModelState.IsValid) 
            return View(categoria);

        _context.Categorias.Update(categoria);
        await _context.SaveChangesAsync();
        TempData["Success"] = $"Categoría '{categoria.Nombre}' actualizada correctamente.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Categorias/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (categoria == null) 
            return NotFound();

        return View(categoria);
    }

    // POST: /Categorias/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria != null)
        {
            var tieneProductos = await _context.Productos.AnyAsync(p => p.CategoriaId == id);
            if (tieneProductos)
            {
                TempData["Error"] = "No se puede eliminar la categoría porque tiene productos asociados.";
                return RedirectToAction(nameof(Index));
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Categoría eliminada exitosamente.";
        }
        return RedirectToAction(nameof(Index));
    }
}
