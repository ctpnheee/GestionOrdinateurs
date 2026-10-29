using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IHttpClientFactory httpClientFactory;
    private readonly IConfiguration configuration;

    public AuthenticationController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        this.httpClientFactory = httpClientFactory;
        this.configuration = configuration;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromQuery] string login)
    {
        var client = httpClientFactory.CreateClient("GestionOrdinateursApi");
        var encodedLogin = Uri.EscapeDataString(login);
        var response = await client.GetAsync($"api/users/{encodedLogin}");

        if (response.StatusCode == HttpStatusCode.NotFound)
            return Unauthorized("Utilisateur inconnu.");

        if (!response.IsSuccessStatusCode)
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "L'API des utilisateurs est indisponible.");

        var user = await response.Content.ReadFromJsonAsync<User>();

        if (user == null)
            return StatusCode(
                StatusCodes.Status502BadGateway,
                "La réponse de l'API des utilisateurs est invalide.");

        return Ok(GenerateJwtToken(user));
    }

    private string GenerateJwtToken(User user)
    {
        var jwtKey = configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("La configuration Jwt:Key est manquante.");
        var key = Encoding.UTF8.GetBytes(jwtKey);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
