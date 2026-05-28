using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Santtoary.API.Helpers;
using Santtoary.Shared.DTOs;
using Santtoary.Shared.Entidades;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Santtoary.API.Controllers
{
    
        [ApiController]
        [Route("/api/accounts")]
    public class AccountsController : ControllerBase
        {
            private readonly IUserHelper _userHelper;
            private readonly IConfiguration _configuration;

            public AccountsController(IUserHelper userHelper, IConfiguration configuration)
            {
                _userHelper = userHelper;
                _configuration = configuration;
            }

            [HttpPost("CreateUser")]
            public async Task<ActionResult> CreateUser([FromBody] UserDTO model)
            {
                User user = model;
                var result = await _userHelper.AddUserAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userHelper.AddUserToRoleAsync(user, user.Rol!);
                    return Ok(BuildToken(user));
                }
                return BadRequest(result.Errors.FirstOrDefault());
            }

            [HttpPost("Login")]
            public async Task<ActionResult> Login([FromBody] LoginDTO model)
            {
                var result = await _userHelper.LoginAsync(model);
                if (result.Succeeded)
                {
                    var user = await _userHelper.GetUserAsync(model.Email);
                    return Ok(BuildToken(user));
                }
                if (result.IsLockedOut)
                {
                    return BadRequest("Ha superado el máximo número de intentos, su cuenta está bloqueada, intente de nuevo en 5 minutos.");
                }
                return BadRequest("Email o contraseña incorrectos.");
            }

            [HttpPut]
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            public async Task<ActionResult> Put(User user)
            {
                try
                {
                    var currentUser = await _userHelper.GetUserAsync(user.Email!);
                    if (currentUser == null)
                    {
                        return NotFound();
                    }
                    currentUser.Rol = user.Rol;
                    currentUser.Photo = !string.IsNullOrEmpty(user.Photo) ? user.Photo : currentUser.Photo;

                    var result = await _userHelper.UpdateUserAsync(currentUser);
                    if (result.Succeeded)
                    {
                        return Ok(BuildToken(currentUser));
                    }
                    return BadRequest(result.Errors.FirstOrDefault());
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }

            [HttpGet]
            [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
            public async Task<ActionResult> Get()
            {
                return Ok(await _userHelper.GetUserAsync(User.Identity!.Name!));
            }

            private TokenDTO BuildToken(User user)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email!),
                new Claim(ClaimTypes.Role, user.Rol ?? "Cliente"),
                new Claim("Photo", user.Photo ?? string.Empty),
            };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["jwtKey"]!));
                var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                var expiration = DateTime.UtcNow.AddDays(30);
                var token = new JwtSecurityToken(
                    issuer: null,
                    audience: null,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: credentials);

                return new TokenDTO
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = expiration
                };
            }
    }
}


