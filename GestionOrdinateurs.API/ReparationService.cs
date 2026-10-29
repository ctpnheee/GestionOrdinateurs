using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

public class ReparationService
{
    // Représente les informations utiles dans l'historique d'une réparation.
    public record HistoriqueReparationRecord(
        int OrdinateurId,
        string NomOrdinateur,
        string Technicien,
        string TypeReparation,
        DateTime DateIntervention
    );

    private readonly ParcInformatiqueContext _context;
    private readonly JournalService journalService;
    private readonly object verrou = new object();

    public ReparationService(
        ParcInformatiqueContext context,
        JournalService journalService)
    {
        this._context = context;
        this.journalService = journalService;
    }

    public Task<List<Reparation>> Lister()
    {
        // Include récupère aussi l'ordinateur lié à chaque réparation.
        return _context.Reparations
            .Include(reparation => reparation.UnOrdinateur)
            .ToListAsync();
    }

    public async Task SimulerReparationAsync(Ordinateur ordinateur)
    {
        // Création d'une réparation de test.
        var reparation = new Reparation
        {
            OrdinateurId = ordinateur.Id,
            UnOrdinateur = ordinateur,
            TypeReparation = "Test",
            DateDebut = DateTime.Now,
            Statut = "En cours",
            Technicien = $"Technicien {ordinateur.Id}"
        };

        // Task.WhenAll lance plusieurs réparations avec le même contexte.
        // Le verrou évite les accès simultanés à la base de données.
        lock (verrou)
        {
            _context.Reparations.Add(reparation);
            // SaveChanges est synchrone car await ne peut pas être utilisé dans un lock.
            _context.SaveChanges();
        }

        await Task.Delay(2000);

        lock (verrou)
        {
            reparation.Statut = "Terminée";
            _context.SaveChanges();
        }

        Console.WriteLine($"{ordinateur.Nom} réparé");
        // La réparation terminée est ajoutée au journal avec la date du jour.
        journalService.Ecrire($"{ordinateur.Nom} réparé");
    }
    public Task<List<HistoriqueReparationRecord>> RenvoyerHistorique()
    {
        // Select transforme chaque réparation en une ligne d'historique.
        return _context.Reparations
            .Select(reparation => new HistoriqueReparationRecord(
                reparation.OrdinateurId,
                reparation.UnOrdinateur.Nom,
                reparation.Technicien,
                reparation.TypeReparation,
                reparation.DateDebut
            ))
            .ToListAsync();
    }
}

