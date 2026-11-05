using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionOrdinateurs.Client.Authentication;

public sealed class KeycloakAuthenticationStateProvider(
    ISessionStorageService sessionStorage) : AuthenticationStateProvider
{
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Le client décode le jeton uniquement pour construire son interface.
        // La signature et les autorisations sont toujours contrôlées côté serveur.
        var token = await sessionStorage.GetItemAsync<string>("jwtToken");

        if (string.IsNullOrWhiteSpace(token))
            return new AuthenticationState(Anonymous);

        var handler = new JwtSecurityTokenHandler();

        if (!handler.CanReadToken(token))
            return new AuthenticationState(Anonymous);

        var jwt = handler.ReadJwtToken(token);

        if (jwt.ValidTo <= DateTime.UtcNow)
        {
            await sessionStorage.RemoveItemAsync("jwtToken");
            return new AuthenticationState(Anonymous);
        }

        var claims = jwt.Claims.ToList();
        var username = claims.FirstOrDefault(
            claim => claim.Type == "preferred_username")?.Value;

        if (!string.IsNullOrWhiteSpace(username))
            claims.Add(new Claim(ClaimTypes.Name, username));

        // Blazor attend ClaimTypes.Role pour ses composants d'autorisation.
        foreach (var role in claims.Where(claim => claim.Type == "roles").ToList())
            claims.Add(new Claim(ClaimTypes.Role, role.Value));

        var identity = new ClaimsIdentity(
            claims,
            "Keycloak",
            ClaimTypes.Name,
            ClaimTypes.Role);

        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task MarkUserAsAuthenticatedAsync(string token)
    {
        await sessionStorage.SetItemAsync("jwtToken", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkUserAsLoggedOutAsync()
    {
        await sessionStorage.RemoveItemAsync("jwtToken");
        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(Anonymous)));
    }
}
