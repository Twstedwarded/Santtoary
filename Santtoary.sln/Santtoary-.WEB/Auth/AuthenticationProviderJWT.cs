using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Santtoary_.WEB.Helpers;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace Santtoary_.WEB.Auth
{
    public class AuthenticationProviderJWT : AuthenticationStateProvider, ILoginService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly HttpClient _httpClient;
        private readonly string _tokenKey;
        private readonly AuthenticationState _anonimous;

        public AuthenticationProviderJWT(IJSRuntime jsRuntime, HttpClient httpClient)
        {
            _jsRuntime = jsRuntime;
            _httpClient = httpClient;
            _tokenKey = "TOKEN_KEY";
            _anonimous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _jsRuntime.GetLocalStorage(_tokenKey);
            if (token is null)
            {
                return _anonimous;
            }
            return BuildAuthenticationState(token.ToString()!);
        }

        private AuthenticationState BuildAuthenticationState(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("bearer", token);
            var claims = ParseClaimsFromJWT(token);
            return new AuthenticationState(
                new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
        }

        private IEnumerable<Claim> ParseClaimsFromJWT(string token)
        {
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var unserializedToken = jwtSecurityTokenHandler.ReadJwtToken(token);
            var claims = unserializedToken.Claims.ToList();

            // El token viene con el nombre largo del claim de rol
            // Lo mapeamos explícitamente a ClaimTypes.Role
            var roleClaims = claims
                .Where(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                         || c.Type == "role")
                .Select(c => new Claim(ClaimTypes.Role, c.Value))
                .ToList();

            // Quitamos los originales y añadimos los mapeados
            claims.RemoveAll(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
                               || c.Type == "role");
            claims.AddRange(roleClaims);

            return claims;
        }

        public async Task LoginAsync(string token)
        {
            await _jsRuntime.SetLocalStorage(_tokenKey, token);
            var authState = BuildAuthenticationState(token);
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
        }

        public async Task LogoutAsync()
        {
            await _jsRuntime.RemoveLocalStorage(_tokenKey);
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(Task.FromResult(_anonimous));
        }
    }
}

