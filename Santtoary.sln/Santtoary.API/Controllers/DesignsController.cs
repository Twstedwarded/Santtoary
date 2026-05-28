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
    public class DesignsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DesignsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Ver todos los diseños del catálogo
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            // El .Include(x => x.Artist) es el secreto para que el nombre aparezca
            return Ok(await _context.Designs.Include(x => x.Artist).ToListAsync());
        }

        // 2. POST: Subir un diseño nuevo al portafolio
        [HttpPost]
        public async Task<ActionResult> Post(Design design)
        {
            _context.Designs.Add(design);
            await _context.SaveChangesAsync();
            return Ok(design);
        }

        // 3. PUT: Editar información de un diseño (ej. cambiar el precio o la descripción)
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var design = await _context.Designs.FindAsync(id);
            if (design == null) return NotFound();
            return Ok(design);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, Design design)
        {
            if (id != design.Id) return BadRequest("El ID no coincide.");

            var designExistente = await _context.Designs.FindAsync(id);
            if (designExistente == null) return NotFound();

            // El truco ninja: Copia todos los datos del formulario a la base de datos automáticamente
            _context.Entry(designExistente).CurrentValues.SetValues(design);

            await _context.SaveChangesAsync();
            return Ok();
        }

        // 4. DELETE: Eliminar un diseño del catálogo
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var design = await _context.Designs.FirstOrDefaultAsync(x => x.Id == id);
            if (design == null)
            {
                return NotFound();
            }

            _context.Designs.Remove(design);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}