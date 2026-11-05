using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// L'URL interne sert aux communications entre conteneurs. L'URL externe
// correspond à l'émetteur inscrit dans les jetons reçus du navigateur.
var keycloakInternalUrl = builder.Configuration["Keycloak:InternalUrl"]
    ?? throw new InvalidOperationException("La configuration Keycloak:InternalUrl est manquante.");
var keycloakExternalUrl = builder.Configuration["Keycloak:ExternalUrl"]
    ?? throw new InvalidOperationException("La configuration Keycloak:ExternalUrl est manquante.");
var keycloakRealm = builder.Configuration["Keycloak:Realm"]
    ?? throw new InvalidOperationException("La configuration Keycloak:Realm est manquante.");

// Autorise le client Blazor à appeler la Gateway depuis son propre port.
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5000",
                "http://localhost:5171",
                "https://localhost:7087")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Charge les routes et la destination de l'API définies dans appsettings.json.
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// La Gateway refuse les requêtes sans jeton Keycloak valide avant de les
// transmettre à l'API métier.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidIssuer = $"{keycloakExternalUrl}/realms/{keycloakRealm}",
            ValidateAudience = false,
            ValidateLifetime = true,
            NameClaimType = "preferred_username",
            IssuerSigningKeyResolver = (_, _, _, _) =>
            {
                using var client = new HttpClient();
                var keySetJson = client.GetStringAsync(
                        $"{keycloakInternalUrl}/realms/{keycloakRealm}/protocol/openid-connect/certs")
                    .GetAwaiter()
                    .GetResult();

                return new JsonWebKeySet(keySetJson).GetSigningKeys();
            }
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();

// Transmet les requêtes correspondantes à l'API GestionOrdinateurs.
app.MapReverseProxy().RequireAuthorization();

app.Run();
