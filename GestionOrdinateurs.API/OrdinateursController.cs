using GestionOrdinateurs.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GestionOrdinateurs.API;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OrdinateursController : ControllerBase
{
    // Le contrôleur reçoit ses dépendances grâce à l'injection de dépendances.
    private readonly IOrdinateurService service;
    private readonly ILogger<OrdinateursController> logger;
    private readonly IConfiguration configuration;

    public OrdinateursController(
        IOrdinateurService service,
        ILogger<OrdinateursController> logger,
        IConfiguration configuration)
    {
        this.service = service;
        this.logger = logger;
        this.configuration = configuration;
    }

    [HttpGet]
    // La Task permet d'attendre la liste avant de construire la réponse HTTP.
    public async Task<IActionResult> GetComputers()
    {
        logger.LogInformation("Récupération de la liste des ordinateurs");
        return Ok(await service.Lister());
    }

    [HttpGet("info")]
    public IActionResult GetApplicationInfo()
    {
        logger.LogInformation("Récupération des informations de l'application");

        return Ok(new
        {
            Name = configuration["Application:Name"],
            Version = configuration["Application:Version"]
        });
    }

    [HttpGet("{id}")]
    // La Task permet d'attendre l'ordinateur avant de choisir entre Ok et NotFound.
    public async Task<IActionResult> GetComputerById(int id)
    {
        logger.LogInformation("Récupération de l'ordinateur {OrdinateurId}", id);
        Ordinateur? ordinateur = await service.RenvoyerParId(id);

        if (ordinateur == null)
            return NotFound();

        return Ok(ordinateur);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public IActionResult AddComputer(Ordinateur ordinateur)
    {
        logger.LogInformation("Ajout de l'ordinateur {OrdinateurNom}", ordinateur.Nom);
        service.Ajouter(ordinateur);
        return StatusCode(201, ordinateur);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    // La Task permet d'attendre la mise à jour avant de renvoyer la réponse.
    public async Task<IActionResult> UpdateComputer(int id, Ordinateur ordinateur)
    {
        logger.LogInformation("Mise à jour de l'ordinateur {OrdinateurId}", id);

        if (!await service.MettreAJour(id, ordinateur))
            return NotFound();

        return Ok(await service.RenvoyerParId(id));
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    // La Task permet d'attendre la suppression avant de confirmer le résultat.
    public async Task<IActionResult> DeleteComputer(int id)
    {
        logger.LogInformation("Suppression de l'ordinateur {OrdinateurId}", id);

        if (!await service.Supprimer(id))
            return NotFound();

        return NoContent();
    }

    [HttpGet("Modifier/{marque}")]
    // La Task permet d'attendre les résultats de la recherche avant de les renvoyer.
    public async Task<IActionResult> FindByBrand(string marque)
    {
        logger.LogInformation("Recherche des ordinateurs de la marque {Marque}", marque);
        return Ok(await service.RechercherParMarque(marque));
    }

    [HttpGet("disponibles")]
    // La Task permet d'attendre la liste des ordinateurs disponibles avant de la renvoyer.
    public async Task<IActionResult> GetAvailableComputers()
    {
        logger.LogInformation("Récupération des ordinateurs disponibles");
        return Ok(await service.ListeOrdiDispo());
    }

    [HttpGet("prix-decroissant")]
    // La Task permet d'attendre le classement des ordinateurs avant de le renvoyer.
    public async Task<IActionResult> GetComputersByDescendingPrice()
    {
        logger.LogInformation("Classement des ordinateurs par prix décroissant");
        return Ok(await service.ListeOrdiParPrix());
    }

    [HttpGet("{id}/diagnostic")]
    public async Task<IActionResult> GetDiagnostic(int id)
    {
        Ordinateur? ordinateur = await service.RenvoyerParId(id);

        if (ordinateur == null)
            return NotFound();

        return Ok(service.Diagnostiquer(ordinateur));
    }

    [HttpGet("statistiques")]
    public IActionResult GetStatistiques()
    {
        // Déconstruction du tuple retourné par le service.
        var (total, enStock, valeurStock, nbReparations)
            = service.CalculerStatistiques();

        return Ok(new
        {
            TotalOrdinateur = total,
            OrdiDisponibles = enStock,
            ValeurStock = valeurStock,
            NbReparations = nbReparations
        });
    }

    [HttpGet("inventaire/{nom}")]
    public async Task<IActionResult> Chercher(string nom)
    {
        var inventaire = new InventaireOrdinateurs
        {
            Ordinateurs = await service.Lister()
        };

        // L'indexeur recherche directement l'ordinateur avec son nom.
        Ordinateur? ordinateur = inventaire[nom];

        if (ordinateur == null)
            return NotFound();

        return Ok(ordinateur);
    }

    [HttpGet("inventaire/id/{id}")]
    public async Task<IActionResult> ChercherParId(int id)
    {
        var inventaire = new InventaireOrdinateurs
        {
            Ordinateurs = await service.Lister()
        };

        // L'indexeur recherche directement l'ordinateur avec son identifiant.
        Ordinateur? ordinateur = inventaire[id];

        if (ordinateur == null)
            return NotFound();

        return Ok(ordinateur);
    }

}
