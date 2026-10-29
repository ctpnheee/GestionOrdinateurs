namespace GestionOrdinateurs.Shared;

public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = new();

    // Recherche un ordinateur avec son identifiant.
    public Ordinateur? this[int ordinateurId]
    {
        get
        {
            return Ordinateurs
                .FirstOrDefault(ordinateur => ordinateur.Id == ordinateurId);
        }
    }

    // Recherche un ordinateur avec son nom.
    public Ordinateur? this[string nom]
    {
        get
        {
            return Ordinateurs
                .FirstOrDefault(ordinateur => ordinateur.Nom == nom);
        }
    }
}
