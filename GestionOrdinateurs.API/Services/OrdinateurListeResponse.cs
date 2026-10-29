namespace GestionOrdinateurs.API.Services;

/// <summary>Informations affichées dans la liste des ordinateurs en vente.</summary>
public sealed record OrdinateurListeResponse(
    int Id,
    string Nom,
    decimal Prix,
    string Devise,
    bool Vendu);
