using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Santtoary_.WEB;
using Santtoary_.WEB.Auth;
using Santtoary_.WEB.Repositories;
using Santtoary_.WEB.Helpers; // Tu carpeta Helpers


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var apiBaseUrl = "https://santtoary.azurewebsites.net";


builder.Services.AddScoped<Santtoary_.WEB.Helpers.AuthorizationMessageHandler>();


builder.Services.AddHttpClient("Santtoary.API", client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<Santtoary_.WEB.Helpers.AuthorizationMessageHandler>();


builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("Santtoary.API"));

builder.Services.AddScoped<IRepository, Repository>();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationProviderJWT>();
builder.Services.AddScoped<AuthenticationStateProvider, AuthenticationProviderJWT>(
    x => x.GetRequiredService<AuthenticationProviderJWT>());
builder.Services.AddScoped<ILoginService, AuthenticationProviderJWT>(
    x => x.GetRequiredService<AuthenticationProviderJWT>());

await builder.Build().RunAsync();