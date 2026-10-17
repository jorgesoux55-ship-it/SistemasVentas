using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;
using SistemaVentas.Data;

namespace SistemaVentas.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductosApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<ActionResult<List<Producto>>> GetAll()
        {
            return await _context.Producto
                .Include(p => p.Categoria)
                .OrderBy(p => p.Nombre)
                .ToListAsync();
        }

        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Producto>> GetById(int id)
        {
            var producto = await _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                return NotFound();

            return producto;
        }

        // POST: api/Productos
        [HttpPost]
        public async Task<ActionResult<Producto>> Create(Producto producto)
        {
            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = producto.Id }, producto);
        }

        // PUT: api/Productos/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Producto producto)
        {
            if (id != producto.Id)
                return BadRequest("El id de la url no coincide con el del cuerpo");

            if (!await _context.Producto.AnyAsync(p => p.Id == id))
                return NotFound();

            _context.Producto.Update(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto == null)
                return NotFound();

            if (await _context.DetalleVentas.AnyAsync(d => d.ProductoId == id))
                return Conflict("No se puede eliminar: el producto tiene ventas registradas");

            _context.Producto.Remove(producto);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}