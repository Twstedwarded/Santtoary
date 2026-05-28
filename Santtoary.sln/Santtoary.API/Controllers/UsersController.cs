using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Santtoary.API.DATA;
using Santtoary.Shared.Entidades;
using Microsoft.AspNetCore.Identity;
using Santtoary.Shared.DTOs;
using Santtoary.API.Helpers; // IMPORTANTE: Agregamos esto para que reconozca el MailHelper

namespace Santtoary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMailHelper _mailHelper; // Agregado para enviar correos
        private readonly IConfiguration _configuration; // Agregado para leer el puerto 7000

        // Inyectamos las nuevas herramientas en el constructor
        public UsersController(ApplicationDbContext context, UserManager<User> userManager, IMailHelper mailHelper, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _mailHelper = mailHelper;
            _configuration = configuration;
        }

        // Obtener todos los usuarios para la tabla principal
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _context.Users.ToListAsync());
        }

        // IMPORTANTE: Obtener un usuario por ID (necesario para EDITAR)
        [HttpGet("{id}")]
        public async Task<ActionResult> Get(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        // Crear un nuevo usuario desde el panel y ENVIAR CORREO
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] UserDTO model)
        {
            User user = new User
            {
                UserName = model.UserName,
                Email = model.Email,
                Rol = model.Rol,
                Photo = model.Photo
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // 1. Generamos el token de confirmación de correo
                var myToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                // 2. Armamos el link que el usuario va a clickear
                var tokenLink = Url.Action("ConfirmEmail", "users", new
                {
                    userid = user.Id,
                    token = myToken
                }, HttpContext.Request.Scheme, _configuration["UrlWEB"]);

                // 3. Enviamos el correo usando nuestra clase MailHelper
                var response = _mailHelper.SendMail(user.UserName, user.Email!,
                    $"Santtoary - Confirmación de cuenta",
                    $"<h1>Santtoary - Confirmación de cuenta</h1>" +
                    $"<p>Para habilitar el usuario, por favor hacer clic en 'Confirmar Email':</p>" +
                    $"<b><a href ={tokenLink}>Confirmar Email</a></b>");

                if (response.IsSuccess)
                {
                    return Ok(user);
                }

                return BadRequest(response.Message);
            }

            var errorMessages = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(errorMessages);
        }

        // NUEVO MÉTODO: Para recibir el clic del correo y confirmar la cuenta
        [HttpGet("ConfirmEmail")]
        public async Task<ActionResult> ConfirmEmailAsync(string userId, string token)
        {
            token = token.Replace(" ", "+");
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound();
            }

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors.FirstOrDefault());
            }

            return NoContent();
        }

        // Validar el Login
        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] LoginDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return BadRequest("Usuario o contraseña incorrectos.");
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, model.Password);
            if (isPasswordValid)
            {
                // Verificamos si ya confirmó el correo
                if (!user.EmailConfirmed)
                {
                    return BadRequest("El usuario no ha sido habilitado, debes de seguir las instrucciones del correo enviado para poder habilitar el usuario.");
                }

                return Ok();
            }

            return BadRequest("Usuario o contraseña incorrectos.");
        }

        // Actualizar un usuario existente
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(string id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest();
            }

            var userDb = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (userDb == null)
            {
                return NotFound();
            }

            userDb.UserName = user.UserName;
            userDb.Email = user.Email;
            userDb.Rol = user.Rol;
            userDb.Photo = user.Photo;

            await _context.SaveChangesAsync();
            return Ok(userDb);
        }

        // Eliminar un usuario
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }
        // Método para reenviar el correo de confirmación
        [HttpPost("ResedToken")]
        public async Task<ActionResult> ResedToken([FromBody] EmailDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
            {
                return NotFound();
            }

            var myToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var tokenLink = Url.Action("ConfirmEmail", "users", new
            {
                userid = user.Id,
                token = myToken
            }, HttpContext.Request.Scheme, _configuration["UrlWEB"]);

            var response = _mailHelper.SendMail(user.UserName, user.Email!,
                $"Santtoary - Confirmación de cuenta",
                $"<h1>Santtoary - Confirmación de cuenta</h1>" +
                $"<p>Para habilitar el usuario, por favor hacer clic en 'Confirmar Email':</p>" +
                $"<b><a href ={tokenLink}>Confirmar Email</a></b>");

            if (response.IsSuccess)
            {
                return NoContent();
            }

            return BadRequest(response.Message);
        }
    }
}