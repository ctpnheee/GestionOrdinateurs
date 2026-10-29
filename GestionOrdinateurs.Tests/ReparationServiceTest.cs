using GestionOrdinateurs.API;
using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.Tests;

public class ReparationServiceTest : IDisposable
{
    private readonly ParcInformatiqueContext context;
    private readonly ReparationService service;

    public ReparationServiceTest()
    {
        var options = new DbContextOptionsBuilder<ParcInformatiqueContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        context = new ParcInformatiqueContext(options);

        string dossierLogs = Path.Combine(
            Path.GetTempPath(),
            "GestionOrdinateurs.Tests");

        service = new ReparationService(context, new JournalService(dossierLogs));
    }

    public void Dispose()
    {
        context.Dispose();
    }

    [Fact]
    public async Task SimulerReparationAsync_CreeUneReparation()
    {
        var ordinateur = new Ordinateur { Nom = "PC à réparer" };
        context.Ordinateurs.Add(ordinateur);
        await context.SaveChangesAsync();

        await service.SimulerReparationAsync(ordinateur);

        Reparation? reparation = await context.Reparations.FirstOrDefaultAsync();

        Assert.NotNull(reparation);
        Assert.Equal(ordinateur.Id, reparation.OrdinateurId);
        Assert.Equal("Terminée", reparation.Statut);
    }
}
