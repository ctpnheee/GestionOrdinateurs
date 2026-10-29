using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReparationController : ControllerBase
{
    private readonly ReparationService reparationService;
    private readonly IOrdinateurService ordinateurService;
    private readonly StockService stockService;

    public ReparationController(
        ReparationService reparationService,
        IOrdinateurService ordinateurService,
        StockService stockService)
    {
        this.reparationService = reparationService;
        this.ordinateurService = ordinateurService;
        this.stockService = stockService;
    }

    [HttpPost("SimulerReparation")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SimulerReparation()
    {
        // Récupération de la liste d'ordinateurs.
        List<Ordinateur> listOrdinateurs = await ordinateurService.Lister();


        if (listOrdinateurs.Count == 0)
            return BadRequest("Aucun ordinateur disponible.");

        // Une tâche de réparation est créée pour chaque ordinateur trouvé.
        await Task.WhenAll(
            listOrdinateurs.Select(ordinateur =>
                reparationService.SimulerReparationAsync(ordinateur))
        );

        return Ok($"{listOrdinateurs.Count} réparations simulées.");
    }

    [HttpGet]
    public async Task<IActionResult> GetReparations()
    {
        List<Reparation> reparations = await reparationService.Lister();

        // La projection évite la boucle entre l'ordinateur et ses réparations.
        return Ok(reparations.Select(reparation => new
        {
            reparation.Id,
            reparation.OrdinateurId,
            reparation.TypeReparation,
            reparation.DateDebut,
            reparation.Statut,
            reparation.Technicien,
            Ordinateur = new
            {
                reparation.UnOrdinateur.Id,
                reparation.UnOrdinateur.Nom,
                reparation.UnOrdinateur.Etat,
                reparation.UnOrdinateur.Stock,
                reparation.UnOrdinateur.Prix,
                reparation.UnOrdinateur.Caracteristiques
            }
        }));
    }

    [HttpGet("GetStockSSD")]
    [Authorize(Roles = "Admin")]
    public IActionResult GetStockSSD()
    {
        Parallel.For(
            0,
            10,
            i => stockService.ConsommerSSD());

        Console.WriteLine($"Stock SSD restant : {stockService.StockSSD}");
        return Ok(stockService.StockSSD);
    }

    [HttpGet("/api/reparations/historique")]
    public async Task<IActionResult> GetHistorique()
    {
        return Ok(await reparationService.RenvoyerHistorique());
    }
}
