using GestionOrdinateurs.API.Data;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public sealed class UserService(ParcInformatiqueContext context) : IUserService
{
    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var users = await context.Users
            .AsNoTracking()
            .OrderBy(user => user.Id)
            .ToListAsync(cancellationToken);

        return users
            .Select(user => new UserResponse(user.Id, user.Login, user.Role.ToString()))
            .ToArray();
    }

    public async Task<UserResponse?> GetByLoginAsync(
        string login,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(user => user.Login == login, cancellationToken);

        return user is null
            ? null
            : new UserResponse(user.Id, user.Login, user.Role.ToString());
    }
}
