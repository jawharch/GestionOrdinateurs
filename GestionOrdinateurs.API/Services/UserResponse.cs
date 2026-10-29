namespace GestionOrdinateurs.API.Services;

/// <summary>Utilisateur renvoyé par l'API de gestion.</summary>
public sealed record UserResponse(int Id, string Login, string Role);
