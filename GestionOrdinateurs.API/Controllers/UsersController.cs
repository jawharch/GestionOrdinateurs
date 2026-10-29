using GestionOrdinateurs.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyList<UserResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<UserResponse>>> GetAll(
        CancellationToken cancellationToken)
    {
        var users = await userService.GetAllAsync(cancellationToken);
        return Ok(users);
    }

    [HttpGet("{login}")]
    [ProducesResponseType<UserResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetByLogin(
        string login,
        CancellationToken cancellationToken)
    {
        var user = await userService.GetByLoginAsync(login, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }
}
