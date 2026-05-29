using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Santtoary.API.DATA;
using Santtoary.Shared.DTOs;
using Santtoary.API.Helpers;
using Santtoary.Shared.Entidades;

namespace Santtoary.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] // Protege por defecto los métodos de este controlador
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IMailHelper _mailHelper;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, UserManager<User> userManager, IMailHelper mailHelper, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _mailHelper = mailHelper;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult> Get()
        {
            return Ok(await _context.Users.ToListAsync());
        }

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

        [AllowAnonymous] // Acceso público para registrarse
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
                    return Ok(user);
                }

                return BadRequest(response.Message);
            }

            var errorMessages = result.Errors.Select(e => e.Description).ToList();
            return BadRequest(errorMessages);
        }

        [AllowAnonymous] // Acceso público para confirmar correo
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

        [AllowAnonymous] // Acceso público para loguearse
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
                if (!user.EmailConfirmed)
                {
                    return BadRequest("El usuario no ha sido habilitado, debes seguir las instrucciones del correo enviado para poder habilitar el usuario.");
                }

                return Ok();
            }

            return BadRequest("Usuario o contraseña incorrectos.");
        }

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

        [AllowAnonymous] // Acceso público para reenviar token
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