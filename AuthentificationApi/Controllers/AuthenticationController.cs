using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthentificationApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthenticationController(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    ILogger<AuthenticationController> logger) : ControllerBase
{
    [HttpPost("login")]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        UserResponse? user;
        try
        {
            using var response = await httpClientFactory.CreateClient("UsersApi")
                .GetAsync($"api/users/{Uri.EscapeDataString(request.Login)}", cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound(new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Utilisateur inexistant."
                });
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "L'API des utilisateurs a répondu avec le statut HTTP {StatusCode}.",
                    (int)response.StatusCode);
                return Problem(
                    statusCode: StatusCodes.Status502BadGateway,
                    title: "Le service des utilisateurs a renvoyé une erreur.");
            }

            user = await response.Content.ReadFromJsonAsync<UserResponse>(
                cancellationToken: cancellationToken);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "Impossible de contacter l'API des utilisateurs.");
            return Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Le service des utilisateurs est indisponible.");
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "Réponse JSON invalide de l'API des utilisateurs.");
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Réponse invalide du service des utilisateurs.");
        }

        if (user is null || user.Role is not ("Admin" or "Gestionnaire"))
        {
            logger.LogError("L'API des utilisateurs a renvoyé une réponse invalide.");
            return Problem(
                statusCode: StatusCodes.Status502BadGateway,
                title: "Réponse invalide du service des utilisateurs.");
        }

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTimeOffset.UtcNow.AddMinutes(15);
        var token = new JwtSecurityToken(
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Role)
            ],
            expires: expires.UtcDateTime,
            signingCredentials: credentials);

        return Ok(new LoginResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            expires));
    }
}

/// <summary>Login utilisé pour demander un jeton d'accès.</summary>
public sealed record LoginRequest
{
    /// <summary>Login du compte existant.</summary>
    [System.ComponentModel.DataAnnotations.Required]
    public required string Login { get; init; }
}

/// <summary>Jeton d'accès remis après une connexion réussie.</summary>
public sealed record LoginResponse(string Token, DateTimeOffset ExpiresAtUtc);

internal sealed record UserResponse(int Id, string Login, string Role);
