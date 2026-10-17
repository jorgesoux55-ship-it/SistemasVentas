using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaVentas.Models;
using SistemaVentas.Data;

namespace SistemaVentas.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ClientesApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
        public async Task<ActionResult<List<Cliente>>> GetAll()
        {
            return await _context.Clientes.OrderBy(c => c.Nombre).ToListAsync();
        }

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> GetById(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            return cliente;
        }

        // POST: api/Clientes
        [HttpPost]
        public async Task<ActionResult<Cliente>> Create(Cliente cliente)
        {
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
        }

        // PUT: api/Clientes/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, Cliente cliente)
        {
            if (id != cliente.Id)
                return BadRequest("El id de la url no coincide con el del cuerpo");

            if (!await _context.Clientes.AnyAsync(c => c.Id == id))
                return NotFound();

            _context.Clientes.Update(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound();

            if (await _context.Ventas.AnyAsync(v => v.ClienteId == id))
                return Conflict("No se puede eliminar: el cliente tiene ventas registradas");

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}