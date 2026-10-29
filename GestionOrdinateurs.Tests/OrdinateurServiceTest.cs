using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.Tests;

public class OrdinateurServiceTest : IDisposable
{
    private readonly ParcInformatiqueContext context;
    private readonly OrdinateurService service;

    public OrdinateurServiceTest()
    {
        context = CreerContexte();
        string dossierLogs = Path.Combine(
            Path.GetTempPath(),
            "GestionOrdinateurs.Tests");
        service = new OrdinateurService(context, new JournalService(dossierLogs));
    }

    private ParcInformatiqueContext CreerContexte()
    {
        var options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        return new ParcInformatiqueContext(options);
    }

    public void Dispose()
    {
        context.Dispose();
    }

    [Fact]
    public async Task Ajouter_AjouteUnOrdinateur()
    {
        Ordinateur ordinateur = new Ordinateur
        {
            Nom = "PC Test",
            Stock = 3,
            Prix = 1200m
        };

        service.Ajouter(ordinateur);

        Ordinateur? ordinateurAjoute = await service.RenvoyerParId(ordinateur.Id);
        Assert.NotNull(ordinateurAjoute);
        Assert.Equal(3, ordinateurAjoute.Stock);
        Assert.Equal(1200m, ordinateurAjoute.Prix);
        await service.Supprimer(ordinateur.Id);
    }

    [Fact]
    public async Task Supprimer_OrdinateurExistant_RetourneTrue()
    {
        Ordinateur ordinateur = new Ordinateur { Nom = "PC a supprimer" };
        service.Ajouter(ordinateur);

        bool resultat = await service.Supprimer(ordinateur.Id);

        Assert.True(resultat);
        Assert.DoesNotContain(ordinateur, await service.Lister());
    }

    [Fact]
    public async Task Supprimer_OrdinateurInexistant_RetourneFalse()
    {
        bool resultat = await service.Supprimer(-1);

        Assert.False(resultat);
    }

    [Fact]
    public async Task Rechercher_Ordinateur_ParMarque()
    {
        Ordinateur ordinateur = new Ordinateur
        {
            Nom = "PC recherche",
            Caracteristiques = new Caracteristiques
            {
                Marque = "Marque Test"
            }
        };
        service.Ajouter(ordinateur);

        List<Ordinateur> resultat = await service.RechercherParMarque("Marque Test");

        Assert.Contains(resultat, item => item.Id == ordinateur.Id);
        await service.Supprimer(ordinateur.Id);
    }

    [Fact]
    public async Task ListeOrdiDispo_RenvoieUniquementLesOrdinateursEnStock()
    {
        Ordinateur disponible = new Ordinateur { Nom = "PC disponible", Stock = 2 };
        Ordinateur indisponible = new Ordinateur { Nom = "PC indisponible", Stock = 0 };
        service.Ajouter(disponible);
        service.Ajouter(indisponible);

        List<Ordinateur> resultat = await service.ListeOrdiDispo();

        Assert.Contains(disponible, resultat);
        Assert.DoesNotContain(indisponible, resultat);
    }

    [Fact]
    public async Task ListeOrdiParPrix_RenvoieLesOrdinateursDuPlusCherAuMoinsCher()
    {
        service.Ajouter(new Ordinateur { Nom = "PC 1", Prix = 500m });
        service.Ajouter(new Ordinateur { Nom = "PC 2", Prix = 1500m });
        service.Ajouter(new Ordinateur { Nom = "PC 3", Prix = 900m });

        List<Ordinateur> resultat = await service.ListeOrdiParPrix();

        Assert.Equal(new[] { 1500m, 900m, 500m }, resultat.Select(o => o.Prix));
    }

    [Fact]
    public async Task MettreAJour_ModifieLeStockEtLePrix()
    {
        Ordinateur ordinateur = new Ordinateur { Nom = "PC à modifier" };
        service.Ajouter(ordinateur);

        Ordinateur nouvellesValeurs = new Ordinateur
        {
            Nom = ordinateur.Nom,
            Stock = 5,
            Prix = 1800m,
            Caracteristiques = new Caracteristiques
            {
                RamGo = 32,
                TypeOrdi = TypeOrdinateur.Portable,
                StockageGo = 1000,
                Marque = "Marque Test",
                Processeur = "Processeur Test"
            }
        };

        bool resultat = await service.MettreAJour(ordinateur.Id, nouvellesValeurs);
        Ordinateur? ordinateurModifie = await service.RenvoyerParId(ordinateur.Id);

        Assert.True(resultat);
        Assert.NotNull(ordinateurModifie);
        Assert.Equal(5, ordinateurModifie.Stock);
        Assert.Equal(1800m, ordinateurModifie.Prix);
    }
}
