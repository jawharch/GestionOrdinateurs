namespace GestionOrdinateurs.API.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<UserResponse?> GetByLoginAsync(string login, CancellationToken cancellationToken);
}
