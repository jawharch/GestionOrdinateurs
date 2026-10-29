using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.API.Services.Events;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdinateursController : ControllerBase
{
    private readonly IOrdinateurService _service;
    private readonly ILogger<OrdinateursController> _logger;
    private readonly IConfiguration _configuration;
    private readonly JournalService _journal;

    public OrdinateursController(
        IOrdinateurService service,
        ILogger<OrdinateursController> logger,
        IConfiguration configuration,
        JournalService journal)
    {
        _service = service;
        _logger = logger;
        _configuration = configuration;
        _journal = journal;

        _service.OrdinateurAjoute += AfficherMessageOrdinateurAjoute;
        _service.OrdinateurSupprime += AfficherMessageOrdinateurSupprime;
        _service.OrdinateurMisAJour += AfficherMessageOrdinateurMisAJour;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Récupération de la liste des ordinateurs.");
        return Ok(await _service.ListerAsync(cancellationToken));
    }

    [HttpGet("liste")]
    [ProducesResponseType<IReadOnlyList<OrdinateurListeResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<OrdinateurListeResponse>>> GetListe(
        CancellationToken cancellationToken)
    {
        var ordinateurs = await _service.ListerAsync(cancellationToken);
        IReadOnlyList<OrdinateurListeResponse> response = ordinateurs
            .Select(ordinateur => new OrdinateurListeResponse(
                ordinateur.Id,
                ordinateur.Nom,
                ordinateur.Prix,
                "€",
                ordinateur.Vendu))
            .ToArray();
        return Ok(response);
    }

    [HttpGet("info")]
    public IActionResult GetInfo()
    {
        _logger.LogInformation("Récupération des informations de l'application.");
        var application = _configuration.GetSection("Application");
        return Ok(new
        {
            Name = application["Name"],
            Version = application["Version"]
        });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetComputerById(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recherche de l'ordinateur {OrdinateurId}.", id);
        var ordinateur = await _service.ObtenirParIdAsync(id, cancellationToken);
        return ordinateur == null ? NotFound() : Ok(ordinateur);
    }

    [HttpGet("marque/{marque}")]
    public async Task<IActionResult> GetByMarque(string marque, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recherche des ordinateurs de marque {Marque}.", marque);
        return Ok(await _service.ParMarqueAsync(marque, cancellationToken));
    }

    [HttpGet("disponibles")]
    public async Task<IActionResult> GetDisponibles(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Récupération des ordinateurs disponibles.");
        return Ok(await _service.ListerDisponiblesAsync(cancellationToken));
    }

    [HttpGet("prix")]
    public async Task<IActionResult> GetParPrixDecroissant(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Récupération des ordinateurs triés par prix décroissant.");
        return Ok(await _service.ListerParPrixDecroissantAsync(cancellationToken));
    }

    [HttpGet("{id:int}/diagnostic")]
    public async Task<IActionResult> Diagnostiquer(int id, CancellationToken cancellationToken)
    {
        var ordinateur = await _service.ObtenirParIdAsync(id, cancellationToken);
        return ordinateur == null ? NotFound() : Ok(_service.Diagnostiquer(ordinateur));
    }

    [HttpGet("statistiques")]
    public IActionResult GetStatistiques()
    {
        var (total, enStock, valeurStock, nombreReparations) = _service.CalculerStatistiques();
        return Ok(new
        {
            TotalOrdinateur = total,
            OrdiDisponibles = enStock,
            ValeurStock = valeurStock,
            NbReparations = nombreReparations
        });
    }

    [HttpGet("inventaire/{nom}")]
    public async Task<IActionResult> ChercherParNom(string nom, CancellationToken cancellationToken)
    {
        var inventaire = new InventaireOrdinateurs
        {
            Ordinateurs = await _service.ListerAsync(cancellationToken)
        };
        var ordinateur = inventaire[nom];
        return ordinateur == null ? NotFound() : Ok(ordinateur);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create(Ordinateur ordinateur, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Demande de création de l'ordinateur {OrdinateurId}.", ordinateur.Id);
        if (!await _service.AjouterAsync(ordinateur, cancellationToken))
        {
            return BadRequest("Id déjà utilisé");
        }

        await _journal.EcrireOrdinateurAsync($"Ordinateur {ordinateur.Nom} ajouté", cancellationToken);
        return CreatedAtAction(nameof(GetComputerById), new { id = ordinateur.Id }, ordinateur);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, Ordinateur ordinateur, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Demande de mise à jour de l'ordinateur {OrdinateurId}.", id);
        if (!await _service.MettreAJourAsync(id, ordinateur, cancellationToken))
        {
            return NotFound();
        }

        await _journal.EcrireOrdinateurAsync($"Ordinateur {ordinateur.Nom} modifié", cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Demande de suppression de l'ordinateur {OrdinateurId}.", id);
        var ordinateur = await _service.ObtenirParIdAsync(id, cancellationToken);
        if (ordinateur == null || !await _service.SupprimerAsync(id, cancellationToken))
        {
            return NotFound();
        }

        await _journal.EcrireOrdinateurAsync($"Ordinateur {ordinateur.Nom} supprimé", cancellationToken);
        return NoContent();
    }

    private void AfficherMessageOrdinateurAjoute(object? sender, OrdinateurAjouteEventArgs args) =>
        _logger.LogInformation("Ordinateur ajouté : {OrdinateurId} - {Nom}.", args.Ordinateur.Id, args.Ordinateur.Nom);

    private void AfficherMessageOrdinateurSupprime(object? sender, OrdinateurSupprimeEventArgs args) =>
        _logger.LogInformation("Ordinateur supprimé : {OrdinateurId} - {Nom}.", args.Ordinateur.Id, args.Ordinateur.Nom);

    private void AfficherMessageOrdinateurMisAJour(object? sender, OrdinateurMisAJourEventArgs args) =>
        _logger.LogInformation("Ordinateur mis à jour : {OrdinateurId} - {Nom}.", args.Ordinateur.Id, args.Ordinateur.Nom);
}