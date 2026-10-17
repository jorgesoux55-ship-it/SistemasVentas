using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;
using SistemaVentas.Data;

namespace SistemaVentas.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriasApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<List<Categoria>>> GetAll()
        {
            return await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetById(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();

            return categoria;
        }

        // POST: api/Categorias
        [HttpPost]
        public async Task<ActionResult<Categoria>> Create(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = categoria.Id }, categoria);
        }

        // PUT: api/Categorias/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Categoria categoria)
        {
            if (id != categoria.Id)
                return BadRequest("El id de la url no coincide con el del cuerpo");

            if (!await _context.Categorias.AnyAsync(c => c.Id == id))
                return NotFound();

            _context.Categorias.Update(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound();

            if (await _context.Producto.AnyAsync(p => p.CategoriaId == id))
                return Conflict("No se puede eliminar: la categoria tiene productos");

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}