using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace GestionOrdinateurs.Shared;

[Table("Ordinateur")]
public class Ordinateur
{
    public int Id { get; set; }
    public string Nom { get; set; } = "";
    public EtatOrdinateur Etat { get; set; } = EtatOrdinateur.EnService;

    [DefaultValue(0)]
    public int Stock { get; set; }

    [DefaultValue(0)]
    public decimal Prix { get; set; }

    [DefaultValue(false)]
    public bool Vendu { get; set; } = false;

    public Caracteristiques Caracteristiques { get; set; } = new();
    public List<Reparation> Reparations { get; set; } = new();

    public void MettreEnMaintenance()
    {
        Etat = EtatOrdinateur.EnMaintenance;
    }

    public void RemettreEnService()
    {
        Etat = EtatOrdinateur.EnService;
    }
}
