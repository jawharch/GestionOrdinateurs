using GestionOrdinateurs.Shared;
using GestionOrdinateurs.API.Services.Events;

namespace GestionOrdinateurs.API.Services;

public interface IOrdinateurService
{
    event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    Task<List<Ordinateur>> ListerAsync(CancellationToken cancellationToken = default);
    Task<Ordinateur?> ObtenirParIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> AjouterAsync(Ordinateur ordinateur, CancellationToken cancellationToken = default);
    Task<bool> MettreAJourAsync(int id, Ordinateur ordinateur, CancellationToken cancellationToken = default);
    Task<bool> SupprimerAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Ordinateur>> ParMarqueAsync(string marque, CancellationToken cancellationToken = default);
    Task<List<Ordinateur>> ListerDisponiblesAsync(CancellationToken cancellationToken = default);
    Task<List<Ordinateur>> ListerParPrixDecroissantAsync(CancellationToken cancellationToken = default);
    string Diagnostiquer(Ordinateur ordinateur);
    (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)
        CalculerStatistiques();
}