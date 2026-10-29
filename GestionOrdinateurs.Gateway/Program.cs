var builder = WebApplication.CreateBuilder(args);

// Autorise le client Blazor à appeler la Gateway depuis son propre port.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5171",
                "https://localhost:7087")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Charge les routes et la destination de l'API définies dans appsettings.json.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.UseCors("AllowBlazorClient");

// Transmet les requêtes correspondantes à l'API GestionOrdinateurs.
app.MapReverseProxy();

app.Run();
