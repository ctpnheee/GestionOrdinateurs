using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GestionOrdinateurs.API;

public class ParcInformatiqueContextFactory : IDesignTimeDbContextFactory<ParcInformatiqueContext>
{
    public ParcInformatiqueContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ParcInformatiqueContext>()
            .UseSqlite("Data Source=parcinfo.db")
            .Options;

        return new ParcInformatiqueContext(options);
    }
}
