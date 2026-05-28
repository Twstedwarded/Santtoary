using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Santtoary.API.DATA;
using Santtoary.Shared.Entidades;

namespace Santtoary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Protege los controladores existentes
    public class InventoryItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _context.InventoryItems.ToListAsync());
        }

        [HttpPost]
        public async Task<ActionResult> Post(InventoryItem inventoryItem)
        {
            _context.InventoryItems.Add(inventoryItem);
            await _context.SaveChangesAsync();
            return Ok(inventoryItem);
        }

        // CORREGIDO: Ahora busca en InventoryItems (con el nombre correcto)
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }

        // CORREGIDO: Ahora recibe un InventoryItem (con el nombre correcto)
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, InventoryItem inventoryItem)
        {
            if (id != inventoryItem.Id) return BadRequest("El ID no coincide.");

            var itemExistente = await _context.InventoryItems.FindAsync(id);
            if (itemExistente == null) return NotFound();

            // Truco ninja aplicado a la entidad correcta
            _context.Entry(itemExistente).CurrentValues.SetValues(inventoryItem);

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var item = await _context.InventoryItems.FirstOrDefaultAsync(x => x.Id == id);
            if (item == null) return NotFound();

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}