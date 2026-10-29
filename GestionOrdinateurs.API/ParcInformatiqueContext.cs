using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API;

public class ParcInformatiqueContext : DbContext
{
    public ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options)
        : base(options)
    {
    }

    public DbSet<Ordinateur> Ordinateurs { get; set; } = null!;
    public DbSet<Reparation> Reparations { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Ordinateur>()
            .HasKey(ordinateur => ordinateur.Id);

        modelBuilder.Entity<Ordinateur>()
            .OwnsOne(ordinateur => ordinateur.Caracteristiques);

        modelBuilder.Entity<Ordinateur>()
            .HasMany(ordinateur => ordinateur.Reparations)
            .WithOne(reparation => reparation.UnOrdinateur);

        modelBuilder.Entity<Ordinateur>()
            .Property(ordinateur => ordinateur.Vendu)
            .HasDefaultValue(false);

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Login)
            .IsUnique();
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlite("Data Source=parcinfo.db");
    }

}
