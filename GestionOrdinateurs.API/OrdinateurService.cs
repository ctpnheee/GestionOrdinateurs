using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

public class OrdinateurService : IOrdinateurService
{
    private readonly ParcInformatiqueContext context;
    private readonly JournalService journalService;

    // Les abonnés seront avertis après chaque modification réussie de la base.
    public event EventHandler<OrdinateurAjouteEventArgs>? OrdinateurAjoute;
    public event EventHandler<OrdinateurSupprimeEventArgs>? OrdinateurSupprime;
    public event EventHandler<OrdinateurMisAJourEventArgs>? OrdinateurMisAJour;

    public OrdinateurService(
        ParcInformatiqueContext context,
        JournalService journalService)
    {
        this.context = context;
        this.journalService = journalService;

        // Abonnement aux événements
        OrdinateurAjoute += AfficherMessageAjout;
        OrdinateurSupprime += AfficherMessageSuppression;
        OrdinateurMisAJour += AfficherMessageMiseAJour;
    }

    // Ces méthodes sont appelées automatiquement quand l'événement correspondant est déclenché.
    private void AfficherMessageAjout(object? sender, OrdinateurAjouteEventArgs e)
    {
        Console.WriteLine($"Ordinateur ajouté : {e.Ordinateur.Nom}");
    }

    private void AfficherMessageSuppression(object? sender, OrdinateurSupprimeEventArgs e)
    {
        Console.WriteLine($"Ordinateur supprimé : {e.Ordinateur.Nom}");
    }

    private void AfficherMessageMiseAJour(object? sender, OrdinateurMisAJourEventArgs e)
    {
        Console.WriteLine($"Ordinateur mis à jour : {e.Ordinateur.Nom}");
    }

    // La Task contiendra la liste lorsque la lecture de la base sera terminée.
    public Task<List<Ordinateur>> Lister()
    {
        return context.Ordinateurs.ToListAsync();
    }

    // La Task contiendra l'ordinateur trouvé, ou null si l'identifiant n'existe pas.
    public Task<Ordinateur?> RenvoyerParId(int id)
    {
        return context.Ordinateurs
            .FirstOrDefaultAsync(ordinateur => ordinateur.Id == id);
    }

    // Ajouter un ordinateur
    public void Ajouter(Ordinateur ordinateur)
    {
        context.Ordinateurs.Add(ordinateur);
        context.SaveChanges();
        journalService.EcrireOrdinateur($"Ordinateur ajouté : {ordinateur.Nom}");
        OrdinateurAjoute?.Invoke(this, new OrdinateurAjouteEventArgs(ordinateur));
    }

    // La Task permet d'attendre la recherche et la suppression avant de renvoyer le résultat.
    public async Task<bool> Supprimer(int id)
    {
        Ordinateur? ordinateur = await context.Ordinateurs
            .FirstOrDefaultAsync(ordinateur => ordinateur.Id == id);

        if (ordinateur == null)
            return false;

        context.Ordinateurs.Remove(ordinateur);
        await context.SaveChangesAsync();
        journalService.EcrireOrdinateur($"Ordinateur supprimé : {ordinateur.Nom}");
        OrdinateurSupprime?.Invoke(this, new OrdinateurSupprimeEventArgs(ordinateur));
        return true;
    }

    // La Task permet d'attendre la recherche et l'enregistrement des modifications.
    public async Task<bool> MettreAJour(int id, Ordinateur nouvelOrdinateur)
    {
        Ordinateur? ordinateur = await context.Ordinateurs
            .FirstOrDefaultAsync(ordinateur => ordinateur.Id == id);

        if (ordinateur == null)
            return false;

        ordinateur.Nom = nouvelOrdinateur.Nom;
        ordinateur.Etat = nouvelOrdinateur.Etat;
        ordinateur.Stock = nouvelOrdinateur.Stock;
        ordinateur.Prix = nouvelOrdinateur.Prix;
        ordinateur.Vendu = nouvelOrdinateur.Vendu;
        ordinateur.Caracteristiques.RamGo = nouvelOrdinateur.Caracteristiques.RamGo;
        ordinateur.Caracteristiques.TypeOrdi = nouvelOrdinateur.Caracteristiques.TypeOrdi;
        ordinateur.Caracteristiques.StockageGo = nouvelOrdinateur.Caracteristiques.StockageGo;
        ordinateur.Caracteristiques.Marque = nouvelOrdinateur.Caracteristiques.Marque;
        ordinateur.Caracteristiques.Processeur = nouvelOrdinateur.Caracteristiques.Processeur;
        await context.SaveChangesAsync();
        journalService.EcrireOrdinateur($"Ordinateur modifié : {ordinateur.Nom}");
        OrdinateurMisAJour?.Invoke(this, new OrdinateurMisAJourEventArgs(ordinateur));
        return true;
    }

    // La Task contiendra les résultats lorsque la recherche LINQ sera terminée.
    public Task<List<Ordinateur>> RechercherParMarque(string marque)
    {
        return context.Ordinateurs
            .Where(ordinateur => ordinateur.Caracteristiques.Marque.ToLower() == marque.ToLower())
            .ToListAsync();
    }

    // La Task contiendra uniquement les ordinateurs encore disponibles en stock.
    public Task<List<Ordinateur>> ListeOrdiDispo()
    {
        return context.Ordinateurs
            .Where(ordinateur => ordinateur.Stock > 0)
            .ToListAsync();
    }

    // La Task contiendra les ordinateurs classés du plus cher au moins cher.
    public async Task<List<Ordinateur>> ListeOrdiParPrix()
    {
        // SQLite stocke les decimal en texte, donc le tri est réalisé en mémoire.
        List<Ordinateur> ordinateurs = await context.Ordinateurs.ToListAsync();

        return ordinateurs
            .OrderByDescending(ordinateur => ordinateur.Prix)
            .ToList();
    }

    // Le pattern matching propose une action selon l'état et la RAM de l'ordinateur.
    public string Diagnostiquer(Ordinateur ordinateur)
    {
        return ordinateur switch
        {
            { Etat: EtatOrdinateur.HorsService }
                => "Remplacement",

            {
                Etat: EtatOrdinateur.Lent,
                Caracteristiques: { RamGo: < 8 }
            }
                => "Ajouter RAM",

            _ => "Aucune action"
        };
    }

    // Le tuple permet de renvoyer plusieurs statistiques dans un seul résultat.
    public (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)
        CalculerStatistiques()
    {
        List<Ordinateur> ordinateurs = context.Ordinateurs.ToList();

        int totalOrdinateur = ordinateurs.Count;
        int ordiDisponibles = ordinateurs.Count(ordinateur => ordinateur.Stock > 0);
        decimal valeurStock = ordinateurs.Sum(ordinateur => ordinateur.Prix * ordinateur.Stock);
        int nbReparations = context.Reparations.Count();

        return (totalOrdinateur, ordiDisponibles, valeurStock, nbReparations);
    }
}
