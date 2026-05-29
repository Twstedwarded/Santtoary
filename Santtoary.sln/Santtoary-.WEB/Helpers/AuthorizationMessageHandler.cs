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
            // ← CAMBIO: de "authToken" a "TOKEN_KEY" para que coincida con AuthenticationProviderJWT
            var token = await _js.InvokeAsync<string>("localStorage.getItem", "TOKEN_KEY");

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
