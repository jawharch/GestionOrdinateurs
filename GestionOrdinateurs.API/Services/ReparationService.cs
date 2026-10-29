using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Services;

public record HistoriqueReparationRecord(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention);

public class ReparationService
{
    private readonly IDbContextFactory<ParcInformatiqueContext> _contextFactory;
    private readonly JournalService _journal;

    public ReparationService(
        IDbContextFactory<ParcInformatiqueContext> contextFactory,
        JournalService journal) =>
        (_contextFactory, _journal) =
        (contextFactory, journal);

    public async Task SimulerReparationAsync(
        Ordinateur ordinateur,
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var reparation = new Reparation
        {
            OrdinateurId = ordinateur.Id,
            TypeReparation = "Diagnostic et réparation",
            DateDebut = DateTime.UtcNow,
            Statut = "En cours",
            Technicien = "Atelier"
        };

        context.Reparations.Add(reparation);
        await context.SaveChangesAsync(cancellationToken);

        await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
        reparation.Statut = "Terminée";
        await context.SaveChangesAsync(cancellationToken);
        await _journal.EcrireReparationAsync($"{ordinateur.Nom} réparé", cancellationToken);
    }

    public async Task<List<Reparation>> ListerAsync(CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Reparations
            .AsNoTracking()
            .Include(reparation => reparation.UnOrdinateur)
            .OrderByDescending(reparation => reparation.DateDebut)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<HistoriqueReparationRecord>> ObtenirHistoriqueAsync(
        CancellationToken cancellationToken = default)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Reparations
            .AsNoTracking()
            .OrderByDescending(reparation => reparation.DateDebut)
            .Select(reparation => new HistoriqueReparationRecord(
                reparation.OrdinateurId,
                reparation.UnOrdinateur.Nom,
                reparation.Technicien,
                reparation.TypeReparation,
                reparation.DateDebut))
            .ToListAsync(cancellationToken);
    }
}
