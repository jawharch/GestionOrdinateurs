using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services.Events;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public class OrdinateurService : IOrdinateurService
{
    private readonly ParcInformatiqueContext _context;

    public OrdinateurService(ParcInformatiqueContext context) => _context = context;

    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    public Task<List<Ordinateur>> ListerAsync(CancellationToken cancellationToken = default) =>
        _context.Ordinateurs.AsNoTracking().ToListAsync(cancellationToken);

    public Task<Ordinateur?> ObtenirParIdAsync(int id, CancellationToken cancellationToken = default) =>
        _context.Ordinateurs.FirstOrDefaultAsync(ordinateur => ordinateur.Id == id, cancellationToken);

    public async Task<bool> AjouterAsync(Ordinateur ordinateur, CancellationToken cancellationToken = default)
    {
        if (await _context.Ordinateurs.AnyAsync(item => item.Id == ordinateur.Id, cancellationToken))
        {
            return false;
        }

        _context.Ordinateurs.Add(ordinateur);
        await _context.SaveChangesAsync(cancellationToken);
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs(ordinateur));
        return true;
    }

    public async Task<bool> MettreAJourAsync(
        int id,
        Ordinateur ordinateur,
        CancellationToken cancellationToken = default)
    {
        var existant = await ObtenirParIdAsync(id, cancellationToken);
        if (existant == null) return false;
        existant.Modifier(
            ordinateur.Nom,
            ordinateur.Marque,
            ordinateur.Caracteristiques,
            ordinateur.Etat,
            ordinateur.Prix,
            ordinateur.Stock,
            ordinateur.Vendu);
        await _context.SaveChangesAsync(cancellationToken);
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs(existant));
        return true;
    }

    public async Task<bool> SupprimerAsync(int id, CancellationToken cancellationToken = default)
    {
        var ordinateur = await ObtenirParIdAsync(id, cancellationToken);
        if (ordinateur == null) return false;
        _context.Ordinateurs.Remove(ordinateur);
        await _context.SaveChangesAsync(cancellationToken);
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs(ordinateur));
        return true;
    }

    public Task<List<Ordinateur>> ParMarqueAsync(string marque, CancellationToken cancellationToken = default) =>
        _context.Ordinateurs.AsNoTracking()
            .Where(ordinateur => ordinateur.Marque.ToLower() == marque.ToLower())
            .ToListAsync(cancellationToken);

    public Task<List<Ordinateur>> ListerDisponiblesAsync(CancellationToken cancellationToken = default) =>
        _context.Ordinateurs.AsNoTracking()
            .Where(ordinateur => ordinateur.Stock > 0)
            .ToListAsync(cancellationToken);

    public Task<List<Ordinateur>> ListerParPrixDecroissantAsync(CancellationToken cancellationToken = default) =>
        _context.Ordinateurs.AsNoTracking()
            .OrderByDescending(ordinateur => ordinateur.Prix)
            .ToListAsync(cancellationToken);

    public string Diagnostiquer(Ordinateur ordinateur) =>
        ordinateur switch
        {
            { Etat: EtatOrdinateur.HorsService } => "Remplacement",
            { Etat: EtatOrdinateur.Lent, Caracteristiques.RamGo: < 8 } => "Ajouter RAM",
            _ => "Aucune action"
        };

    public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)
        CalculerStatistiques()
    {
        var ordinateurs = _context.Ordinateurs.AsNoTracking().ToList();
        return (
            ordinateurs.Count,
            ordinateurs.Count(ordinateur => ordinateur.Stock > 0),
            ordinateurs.Sum(ordinateur => ordinateur.Prix * ordinateur.Stock),
            _context.Reparations.Count());
    }
}