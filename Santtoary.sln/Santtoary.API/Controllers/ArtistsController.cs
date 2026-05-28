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
    public class ArtistsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ArtistsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Trae la lista completa de artistas
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _context.Artists.ToListAsync());
        }

        // 2. POST: Crea un artista nuevo en la base de datos
        [HttpPost]
        public async Task<ActionResult> Post(Artist artist)
        {
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
            return Ok(artist);
        }

        // 3. PUT: Actualiza los datos de un artista que ya existe
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var artist = await _context.Artists.FindAsync(id);
            if (artist == null) return NotFound();
            return Ok(artist);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, Artist artist)
        {
            if (id != artist.Id) return BadRequest("El ID no coincide.");

            var artistaExistente = await _context.Artists.FindAsync(id);
            if (artistaExistente == null) return NotFound();

            // Actualizamos campo por campo
            artistaExistente.Name = artist.Name;
            artistaExistente.Specialty = artist.Specialty;
            artistaExistente.PhoneNumber = artist.PhoneNumber;

            _context.Artists.Update(artistaExistente);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // 4. DELETE: Borra un artista por su ID
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(x => x.Id == id);
            if (artist == null)
            {
                return NotFound();
            }

            _context.Artists.Remove(artist);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
