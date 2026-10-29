namespace GestionOrdinateurs.Shared;

public class Reparation
{
    public int Id { get; set; }

    public int OrdinateurId { get; set; }

    public Ordinateur UnOrdinateur { get; set; }

    public string TypeReparation { get; set; }

    public DateTime DateDebut { get; set; }

    public string Statut { get; set; }

    public string Technicien { get; set; }
}
