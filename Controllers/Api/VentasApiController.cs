using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;
using SistemaVentas.Data;

namespace SistemaVentas.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class VentasApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public VentasApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Ventas
        [HttpGet]
        public async Task<ActionResult<List<Venta>>> GetAll()
        {
            return await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .OrderByDescending(v => v.Fecha)
                .ToListAsync();
        }

        // GET: api/Ventas/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Venta>> GetById(int id)
        {
            var venta = await _context.Ventas
                .Include(v => v.Cliente)
                .Include(v => v.Detalles)
                    .ThenInclude(d => d.Producto)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venta == null)
                return NotFound();

            return venta;
        }

        // POST: api/Ventas
        [HttpPost]
        public async Task<ActionResult<Venta>> Create(Venta venta)
        {
            venta.CalcularTotal();
            venta.CalcularVambio();
            venta.Actualizarstock();

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = venta.Id }, venta);
        }

        // PUT: api/Ventas/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Venta venta)
        {
            if (id != venta.Id)
                return BadRequest("El id de la url no coincide con el del cuerpo");

            if (!await _context.Ventas.AnyAsync(v => v.Id == id))
                return NotFound();

            _context.Ventas.Update(venta);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Ventas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var venta = await _context.Ventas.FindAsync(id);
            if (venta == null)
                return NotFound();

            _context.Ventas.Remove(venta);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}