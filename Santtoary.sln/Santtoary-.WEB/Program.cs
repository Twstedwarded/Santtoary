using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Santtoary_.WEB;
using Santtoary_.WEB.Repositories;
using Santtoary_.WEB.Helpers; // Tu carpeta Helpers

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


var apiBaseUrl = "https://localhost:8000/";


builder.Services.AddScoped<Santtoary_.WEB.Helpers.AuthorizationMessageHandler>();


builder.Services.AddHttpClient("Santtoary.API", client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<Santtoary_.WEB.Helpers.AuthorizationMessageHandler>();


builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("Santtoary.API"));

builder.Services.AddScoped<IRepository, Repository>();

await builder.Build().RunAsync();