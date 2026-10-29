using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.SessionStorage;
using GestionOrdinateurs.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Le token JWT sera conservé dans la session du navigateur.
builder.Services.AddBlazoredSessionStorage();

// Client utilisé uniquement pour obtenir le token JWT.
builder.Services.AddHttpClient("AuthentificationApi", client =>
{
    var baseUrl = builder.Configuration["Services:AuthentificationApi"]
        ?? throw new InvalidOperationException(
            "La configuration Services:AuthentificationApi est manquante.");

    client.BaseAddress = new Uri(baseUrl);
});

// Les listes Ordinateurs et Réparations passent par la Gateway.
builder.Services.AddHttpClient("Gateway", client =>
{
    var baseUrl = builder.Configuration["Services:Gateway"]
        ?? throw new InvalidOperationException(
            "La configuration Services:Gateway est manquante.");

    client.BaseAddress = new Uri(baseUrl);
});

await builder.Build().RunAsync();
