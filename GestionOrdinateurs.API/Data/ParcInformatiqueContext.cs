using GestionOrdinateurs.Shared;
using Microsoft.EntityFrameworkCore;

namespace GestionOrdinateurs.API.Data;

public class ParcInformatiqueContext : DbContext
{
    public ParcInformatiqueContext(DbContextOptions<ParcInformatiqueContext> options)
        : base(options)
    {
    }

    public DbSet<Ordinateur> Ordinateurs => Set<Ordinateur>();
    public DbSet<Reparation> Reparations => Set<Reparation>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .Property(user => user.Login)
            .HasMaxLength(100)
            .IsRequired();

        modelBuilder.Entity<User>()
            .HasIndex(user => user.Login)
            .IsUnique();

        modelBuilder.Entity<Ordinateur>()
            .OwnsOne(ordinateur => ordinateur.Caracteristiques);

        modelBuilder.Entity<Ordinateur>()
            .HasMany(ordinateur => ordinateur.Reparations)
            .WithOne(reparation => reparation.UnOrdinateur)
            .HasForeignKey(reparation => reparation.OrdinateurId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Ordinateur>()
            .Property(ordinateur => ordinateur.Prix)
            .HasPrecision(10, 2)
            .HasDefaultValue(0m);

        modelBuilder.Entity<Ordinateur>()
            .Property(ordinateur => ordinateur.Stock)
            .HasDefaultValue(0);

        modelBuilder.Entity<Ordinateur>()
            .Property(ordinateur => ordinateur.Vendu)
            .HasDefaultValue(false);
    }
}
