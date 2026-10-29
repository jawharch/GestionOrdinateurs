using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace GestionOrdinateurs.Client.Services;

public sealed class SessionAuthenticationStateProvider(
    ISessionStorageService sessionStorage) : AuthenticationStateProvider
{
    private const string TokenKey = "authToken";

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await sessionStorage.GetItemAsync<string>(TokenKey);
        if (string.IsNullOrWhiteSpace(token))
        {
            return Anonymous();
        }

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
        {
            await sessionStorage.RemoveItemAsync(TokenKey);
            return Anonymous();
        }

        var jwt = handler.ReadJwtToken(token);
        if (jwt.ValidTo <= DateTime.UtcNow)
        {
            await sessionStorage.RemoveItemAsync(TokenKey);
            return Anonymous();
        }

        var identity = new ClaimsIdentity(
            jwt.Claims,
            authenticationType: "jwt",
            nameType: ClaimTypes.Name,
            roleType: ClaimTypes.Role);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task SignInAsync(string token)
    {
        await sessionStorage.SetItemAsync(TokenKey, token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task SignOutAsync()
    {
        await sessionStorage.RemoveItemAsync(TokenKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous()));
    }

    private static AuthenticationState Anonymous() =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));
}
