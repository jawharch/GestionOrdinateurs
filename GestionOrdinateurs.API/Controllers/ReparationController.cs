using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Route("api/reparations")]
public class ReparationController : ControllerBase
{
    private readonly ParcInformatiqueContext _context;
    private readonly ReparationService _reparationService;
    private readonly StockService _stockService;
    private readonly ILogger<ReparationController> _logger;

    public ReparationController(
        ParcInformatiqueContext context,
        ReparationService reparationService,
        StockService stockService,
        ILogger<ReparationController> logger)
    {
        _context = context;
        _reparationService = reparationService;
        _stockService = stockService;
        _logger = logger;
    }

    [HttpPost("SimulerReparation")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SimulerReparation(CancellationToken cancellationToken)
    {
        var ordinateurs = await _context.Ordinateurs
            .AsNoTracking()
            .OrderBy(ordinateur => ordinateur.Id)
            .Take(5)
            .ToListAsync(cancellationToken);

        if (ordinateurs.Count < 5)
        {
            return BadRequest("Il faut au moins cinq ordinateurs pour simuler les réparations.");
        }

        await Task.WhenAll(ordinateurs.Select(ordinateur =>
            _reparationService.SimulerReparationAsync(ordinateur, cancellationToken)));

        _logger.LogInformation("Simulation simultanée de {NombreReparations} réparations terminée.",
            ordinateurs.Count);
        return Ok(await _reparationService.ListerAsync(cancellationToken));
    }

    [HttpGet]
    public async Task<ActionResult<List<Reparation>>> GetReparations(CancellationToken cancellationToken) =>
        Ok(await _reparationService.ListerAsync(cancellationToken));

    [HttpGet("historique")]
    public async Task<ActionResult<List<HistoriqueReparationRecord>>> GetHistorique(
        CancellationToken cancellationToken) =>
        Ok(await _reparationService.ObtenirHistoriqueAsync(cancellationToken));

    [HttpGet("GetStockSSD")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> GetStockSSD(CancellationToken cancellationToken)
    {
        await Task.WhenAll(Enumerable.Range(0, 10)
            .Select(_ => _stockService.ConsommerSSDAsync(cancellationToken)));

        _logger.LogInformation("Stock SSD restant après dix consommations parallèles : {StockSSD}.",
            _stockService.StockSSD);
        return Ok(_stockService.StockSSD);
    }
}
