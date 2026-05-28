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
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. ESTE GET ES PARA LA TABLA (Trae la lista con los nombres cruzados)
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _context.Appointments
                .Include(x => x.Client)
                .Include(x => x.Artist)
                .ToListAsync());
        }

        // 2. ESTE GET ES PARA EL BOTÓN EDITAR (Busca una sola cita por su ID)
        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == id);

            if (appointment == null)
            {
                return NotFound();
            }

            return Ok(appointment);
        }

        // 2. POST: Agenda una nueva cita
        [HttpPost]
        public async Task<ActionResult> Post(Appointment appointment)
        {
            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return Ok(appointment);
        }

        // 3. PUT: Actualiza los datos de una cita (ej. cambiar fecha o estado)
        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(int id, Appointment appointment)
        {
            // 1. Verificamos que el ID de la URL sea el mismo del objeto
            if (id != appointment.Id)
            {
                return BadRequest("El ID no coincide.");
            }

            // 2. Buscamos la cita original en la base de datos
            var citaExistente = await _context.Appointments.FindAsync(id);
            if (citaExistente == null)
            {
                return NotFound();
            }

            // 3. Le pasamos los datos nuevos manualmente (a prueba de fallos)
            citaExistente.Date = appointment.Date;
            citaExistente.EstimatedHours = appointment.EstimatedHours;
            citaExistente.TotalPrice = appointment.TotalPrice;
            citaExistente.Status = appointment.Status;
            citaExistente.ClientId = appointment.ClientId;
            citaExistente.ArtistId = appointment.ArtistId;

            // 4. Guardamos los cambios
            _context.Appointments.Update(citaExistente);
            await _context.SaveChangesAsync();

            return Ok();
        }
        // 4. DELETE: Cancela/Borra una cita por su ID
        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(x => x.Id == id);
            if (appointment == null)
            {
                return NotFound();
            }

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}