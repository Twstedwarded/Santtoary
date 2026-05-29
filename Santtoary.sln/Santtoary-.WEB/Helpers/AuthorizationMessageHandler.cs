using Microsoft.JSInterop;
using System.Net.Http.Headers;

namespace Santtoary_.WEB.Helpers
{
    public class AuthorizationMessageHandler : DelegatingHandler
    {
        private readonly IJSRuntime _js;

        public AuthorizationMessageHandler(IJSRuntime js)
        {
            _js = js;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Buscamos el token en el navegador
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

            // Si hay token, lo pegamos en la cabecera (Header)
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            // Enviamos la petición
            return await base.SendAsync(request, cancellationToken);
        }
    }
}