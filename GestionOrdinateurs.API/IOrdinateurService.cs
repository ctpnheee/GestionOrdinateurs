using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API;

public interface IOrdinateurService
{
    Task<List<Ordinateur>> Lister();

    Task<Ordinateur?> RenvoyerParId(int id);

    void Ajouter(Ordinateur ordinateur);

    Task<bool> Supprimer(int id);

    Task<bool> MettreAJour(int id, Ordinateur nouvelOrdinateur);

    Task<List<Ordinateur>> RechercherParMarque(string marque);

    Task<List<Ordinateur>> ListeOrdiDispo();

    Task<List<Ordinateur>> ListeOrdiParPrix();

    string Diagnostiquer(Ordinateur ordinateur);

    (int TotalOrdinateur, int OrdiDisponibles, decimal ValeurStock, int NbReparations)
        CalculerStatistiques();
}
