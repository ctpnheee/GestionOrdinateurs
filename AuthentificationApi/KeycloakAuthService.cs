using System.Text.Json.Serialization;

namespace AuthentificationApi;

public sealed class KeycloakAuthService(
    HttpClient httpClient,
    IConfiguration configuration)
{
    public async Task<KeycloakTokenResponse> LoginAsync(
        string username,
        string password,
        CancellationToken cancellationToken = default)
    {
        var internalUrl = configuration["Keycloak:InternalUrl"]
            ?? throw new InvalidOperationException("Keycloak:InternalUrl est manquant.");
        var realm = configuration["Keycloak:Realm"]
            ?? throw new InvalidOperationException("Keycloak:Realm est manquant.");
        var clientId = configuration["Keycloak:ClientId"]
            ?? throw new InvalidOperationException("Keycloak:ClientId est manquant.");
        var clientSecret = configuration["Keycloak:ClientSecret"]
            ?? throw new InvalidOperationException("Keycloak:ClientSecret est manquant.");

        // Le TP demande un formulaire utilisateur/mot de passe dans Blazor.
        // L'API échange donc ces identifiants contre un jeton auprès de Keycloak.
        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = clientId,
            ["client_secret"] = clientSecret,
            ["username"] = username,
            ["password"] = password
        });

        using var response = await httpClient.PostAsync(
            $"{internalUrl}/realms/{realm}/protocol/openid-connect/token",
            content,
            cancellationToken);

        if (response.StatusCode is System.Net.HttpStatusCode.BadRequest
            or System.Net.HttpStatusCode.Unauthorized)
            throw new UnauthorizedAccessException();

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<KeycloakTokenResponse>(
                   cancellationToken: cancellationToken)
               ?? throw new HttpRequestException("Réponse Keycloak invalide.");
    }
}

public sealed class KeycloakLoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
}
