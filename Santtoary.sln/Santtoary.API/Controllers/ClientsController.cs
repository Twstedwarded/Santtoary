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
    public class ClientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Trae todos los clientes
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _context.Clients.ToListAsync());
        }

        // 2. POST: Crear un nuevo cliente
        [HttpPost]
        public async Task<ActionResult> Post(Client client)
        {
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
            return Ok(client);
        }

        // 3. PUT: Actualizar datos de un cliente
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();
            return Ok(client);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, Client client)
        {
            if (id != client.Id) return BadRequest("El ID no coincide.");

            var clienteExistente = await _context.Clients.FindAsync(id);
            if (clienteExistente == null) return NotFound();

            // Actualizamos campo por campo
            clienteExistente.Name = client.Name;
            clienteExistente.Phone = client.Phone;
            clienteExistente.Email = client.Email;
            clienteExistente.MedicalNotes = client.MedicalNotes;
            clienteExistente.Document = client.Document;

            _context.Clients.Update(clienteExistente);
            await _context.SaveChangesAsync();
            return Ok();
        }

        // 4. DELETE: Borrar un cliente por ID
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var client = await _context.Clients.FirstOrDefaultAsync(x => x.Id == id);
            if (client == null)
            {
                return NotFound();
            }

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}