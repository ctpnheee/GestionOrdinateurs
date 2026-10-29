namespace GestionOrdinateurs.Shared;

public class Caracteristiques
{
    public int RamGo { get; set; }
    public TypeOrdinateur TypeOrdi { get; set; }
    public int StockageGo { get; set; }
    public string Marque { get; set; } = "";
    public string Processeur { get; set; } = "";

    public Caracteristiques()
    {
    }

    public Caracteristiques(
        int ramGo,
        TypeOrdinateur typeOrdi,
        int stockageGo,
        string marque,
        string processeur)
    {
        RamGo = ramGo;
        TypeOrdi = typeOrdi;
        StockageGo = stockageGo;
        Marque = marque;
        Processeur = processeur;
    }
}
