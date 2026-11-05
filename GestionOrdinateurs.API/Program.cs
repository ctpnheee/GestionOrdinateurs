using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Security.Claims;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddDbContext<ParcInformatiqueContext>();
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<ReparationService>();
builder.Services.AddScoped<StockService>();
builder.Services.AddScoped<JournalService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestionOrdinateurs",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Entrez : Bearer {token}"
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
    });
});

// Dans Docker, les clés publiques sont récupérées avec le nom du service
// "keycloak", tandis que l'émetteur du jeton reste l'URL visible du navigateur.
var keycloakInternalUrl = builder.Configuration["Keycloak:InternalUrl"]
    ?? throw new InvalidOperationException("La configuration Keycloak:InternalUrl est manquante.");
var keycloakExternalUrl = builder.Configuration["Keycloak:ExternalUrl"]
    ?? throw new InvalidOperationException("La configuration Keycloak:ExternalUrl est manquante.");
var keycloakRealm = builder.Configuration["Keycloak:Realm"]
    ?? throw new InvalidOperationException("La configuration Keycloak:Realm est manquante.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        ValidateIssuer = true,
        ValidIssuer = $"{keycloakExternalUrl}/realms/{keycloakRealm}",
        ValidateAudience = false,
        ValidateLifetime = true,
        NameClaimType = "preferred_username",
        RoleClaimType = ClaimTypes.Role,
        // Keycloak signe les jetons avec une clé publiée dans son endpoint JWKS.
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
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Le mapper du realm place les rôles dans la claim "roles".
            // On les convertit pour que [Authorize(Roles = "...")] fonctionne.
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                foreach (var roleClaim in context.Principal.FindAll("roles").ToList())
                {
                    if (roleClaim.Value.StartsWith('['))
                    {
                        foreach (var role in JsonSerializer.Deserialize<string[]>(roleClaim.Value) ?? [])
                            identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                    else
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, roleClaim.Value));
                    }
                }
            }

            Console.WriteLine(
                $"Utilisateur utilisant un token valide : {context.Principal?.Identity?.Name ?? "Anonymous"}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = _ =>
        {
            Console.WriteLine("Token invalide");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
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

var app = builder.Build();

// Initialisation de la base de données au lancement de l'API.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();

    db.Database.Migrate();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Login = "user1", Role = RoleEnum.Admin },
            new User { Login = "user2", Role = RoleEnum.Gestionnaire },
            new User { Login = "user3", Role = RoleEnum.Gestionnaire }
        );

        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
