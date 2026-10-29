using GestionOrdinateurs.API.Data;
using GestionOrdinateurs.API.Services;
using GestionOrdinateurs.Shared;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace GestionOrdinateurs.Tests;

public class OrdinateurServiceTest
{
    [Fact]
    public async Task Ajouter_AjouteUnOrdinateurAvecSesCaracteristiques()
    {
        await using var context = CreerContexte();
        var service = new OrdinateurService(context);

        (await service.AjouterAsync(CreerOrdinateur(100))).Should().BeTrue();
        var ordinateur = await service.ObtenirParIdAsync(100);

        ordinateur.Should().NotBeNull();
        ordinateur!.Caracteristiques.RamGo.Should().Be(8);
        ordinateur.Caracteristiques.TypeOrdi.Should().Be(TypeOrdinateur.Portable);
    }

    [Fact]
    public async Task Supprimer_Existant_RetourneTrue()
    {
        await using var context = CreerContexte();
        var service = new OrdinateurService(context);
        await service.AjouterAsync(CreerOrdinateur(1));

        (await service.SupprimerAsync(1)).Should().BeTrue();
        (await service.ObtenirParIdAsync(1)).Should().BeNull();
    }

    [Fact]
    public async Task Supprimer_Inexistant_RetourneFalse()
    {
        await using var context = CreerContexte();
        var service = new OrdinateurService(context);

        (await service.SupprimerAsync(999)).Should().BeFalse();
    }

    [Fact]
    public async Task MettreAJour_ModifieLesDonneesEnBase()
    {
        await using var context = CreerContexte();
        var service = new OrdinateurService(context);
        await service.AjouterAsync(CreerOrdinateur(2));
        var miseAJour = new Ordinateur(
            2, "PC-MAJ", "Dell", CreerCaracteristiques(16), EtatOrdinateur.EnService, 899.99m, 5);

        (await service.MettreAJourAsync(2, miseAJour)).Should().BeTrue();
        var ordinateur = await service.ObtenirParIdAsync(2);
        ordinateur!.Nom.Should().Be("PC-MAJ");
        ordinateur.Marque.Should().Be("Dell");
        ordinateur.Caracteristiques.RamGo.Should().Be(16);
        ordinateur.Prix.Should().Be(899.99m);
        ordinateur.Stock.Should().Be(5);
    }

    [Fact]
    public async Task RechercheEtTri_UtilisentPrixStockEtMarque()
    {
        await using var context = CreerContexte();
        var service = new OrdinateurService(context);
        await service.AjouterAsync(CreerOrdinateur(1, "HP", 100m, 2));
        await service.AjouterAsync(CreerOrdinateur(2, "Acer", 300m, 0));
        await service.AjouterAsync(CreerOrdinateur(3, "acer", 200m, 1));

        (await service.ListerDisponiblesAsync()).Select(ordinateur => ordinateur.Id)
            .Should().BeEquivalentTo([1, 3]);
        (await service.ListerParPrixDecroissantAsync()).Select(ordinateur => ordinateur.Id)
            .Should().ContainInOrder(2, 3, 1);
        (await service.ParMarqueAsync("ACER")).Should().HaveCount(2);
    }

    [Fact]
    public async Task OperationsCrud_DeclenchentLesEvenementsCorrespondants()
    {
        await using var context = CreerContexte();
        var service = new OrdinateurService(context);
        var ordinateur = CreerOrdinateur(3);
        int? idAjoute = null;
        int? idMisAJour = null;
        int? idSupprime = null;
        service.OrdinateurAjoute += (_, args) => idAjoute = args.Ordinateur.Id;
        service.OrdinateurMisAJour += (_, args) => idMisAJour = args.Ordinateur.Id;
        service.OrdinateurSupprime += (_, args) => idSupprime = args.Ordinateur.Id;

        (await service.AjouterAsync(ordinateur)).Should().BeTrue();
        (await service.MettreAJourAsync(3,
            new Ordinateur(3, "PC-MAJ", "Dell", CreerCaracteristiques(16), EtatOrdinateur.EnService)))
            .Should().BeTrue();
        (await service.SupprimerAsync(3)).Should().BeTrue();

        idAjoute.Should().Be(3);
        idMisAJour.Should().Be(3);
        idSupprime.Should().Be(3);
    }

    [Fact]
    public async Task DiagnosticEtStatistiques_UtilisentEtatCaracteristiquesEtReparations()
    {
        await using var context = CreerContexte();
        var service = new OrdinateurService(context);
        await service.AjouterAsync(CreerOrdinateur(1, stock: 2, prix: 50m));
        await service.AjouterAsync(CreerOrdinateur(2, etat: EtatOrdinateur.Lent, ram: 4));
        await service.AjouterAsync(CreerOrdinateur(3, etat: EtatOrdinateur.HorsService));
        context.Reparations.Add(new Reparation { OrdinateurId = 1, TypeReparation = "Ecran" });
        await context.SaveChangesAsync();

        service.Diagnostiquer((await service.ObtenirParIdAsync(2))!).Should().Be("Ajouter RAM");
        service.Diagnostiquer((await service.ObtenirParIdAsync(3))!).Should().Be("Remplacement");
        service.CalculerStatistiques().Should().Be((3, 3, 300m, 1));
    }

    [Fact]
    public void Inventaire_IndexeursRecherchentParNomEtIndex()
    {
        var premier = CreerOrdinateur(1, "Dell");
        var second = CreerOrdinateur(2, "HP");
        var inventaire = new InventaireOrdinateurs { Ordinateurs = [premier, second] };

        inventaire["pc-2"]!.Id.Should().Be(2);
        inventaire[0].Should().BeSameAs(premier);
    }

    private static ParcInformatiqueContext CreerContexte()
    {
        var options = new DbContextOptionsBuilder<ParcInformatiqueContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ParcInformatiqueContext(options);
    }

    private static Ordinateur CreerOrdinateur(
        int id,
        string marque = "Acer",
        decimal prix = 100,
        int stock = 1,
        EtatOrdinateur etat = EtatOrdinateur.EnStock,
        int ram = 8) =>
        new(id, $"PC-{id}", marque, CreerCaracteristiques(ram), etat, prix, stock);

    private static Caracteristiques CreerCaracteristiques(int ram) =>
        new(ram, 512, TypeOrdinateur.Portable, "i5", 15.6, 8);
}

public class ReparationEtStockTests
{
    [Fact]
    public async Task ReparationsSimultanees_SontEnregistreesAvecLeurOrdinateurEtTechnicien()
    {
        var factory = new TestContextFactory();
        var logRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        await using var journal = new JournalService(logRoot);
        await using (var context = factory.CreateDbContext())
        {
            for (var id = 1; id <= 5; id++)
            {
                context.Ordinateurs.Add(CreerOrdinateur(id));
            }

            await context.SaveChangesAsync();
        }

        var service = new ReparationService(factory, journal);
        await using (var context = factory.CreateDbContext())
        {
            var ordinateurs = await context.Ordinateurs.ToListAsync();
            await Task.WhenAll(ordinateurs.Select(ordinateur =>
                service.SimulerReparationAsync(ordinateur)));
        }

        var reparations = await service.ListerAsync();
        reparations.Should().HaveCount(5)
            .And.OnlyContain(reparation =>
                reparation.Statut == "Terminée" &&
                reparation.Technicien == "Atelier" &&
                reparation.UnOrdinateur != null);

        var historique = await service.ObtenirHistoriqueAsync();
        historique.Should().HaveCount(5)
            .And.OnlyContain(entree => entree.NomOrdinateur.StartsWith("PC-"));
        var (ordinateurId, nomOrdinateur, technicien, typeReparation, dateIntervention) = historique[0];
        ordinateurId.Should().BeGreaterThan(0);
        nomOrdinateur.Should().StartWith("PC-");
        technicien.Should().Be("Atelier");
        typeReparation.Should().Be("Diagnostic et réparation");
        dateIntervention.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        await journal.DisposeAsync();
        Directory.Delete(logRoot, recursive: true);
    }

    [Fact]
    public async Task CreerReparation_EnregistreLeTechnicien()
    {
        await using var context = CreerContexte();
        context.Ordinateurs.Add(CreerOrdinateur(1));
        await context.SaveChangesAsync();
        context.Reparations.Add(new Reparation
        {
            OrdinateurId = 1,
            TypeReparation = "Remplacement écran",
            DateDebut = DateTime.UtcNow,
            Statut = "Terminée",
            Technicien = "Camille"
        });

        await context.SaveChangesAsync();

        (await context.Reparations.SingleAsync()).Technicien.Should().Be("Camille");
    }

    [Fact]
    public async Task Journal_EcritUneLigneDateeDansLeDossierReparation()
    {
        var root = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var journal = new JournalService(root);
        await using (journal)
        {
            await journal.EcrireReparationAsync("Dell XPS réparé");
        }

        var fichier = Path.Combine(root, "Logs", "Reparations", "reparations.log");
        var ligne = await File.ReadAllTextAsync(fichier);
        ligne.Should().Contain($"{DateTime.Now:dd/MM/yyyy} - Dell XPS réparé");
        Directory.Delete(root, recursive: true);
    }

    [Fact]
    public async Task ConsommerSSD_Parallelement_NePassePasSousZero()
    {
        var service = new StockService();

        await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => service.ConsommerSSDAsync()));
        service.StockSSD.Should().Be(0);

        await Task.WhenAll(Enumerable.Range(0, 100).Select(_ => service.ConsommerSSDAsync()));

        service.StockSSD.Should().Be(0);
    }

    private static Ordinateur CreerOrdinateur(int id) =>
        new(id, $"PC-{id}", "Acer",
            new Caracteristiques(8, 512, TypeOrdinateur.Portable, "i5"), EtatOrdinateur.EnStock);

    private static ParcInformatiqueContext CreerContexte() =>
        new(new DbContextOptionsBuilder<ParcInformatiqueContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private sealed class TestContextFactory : IDbContextFactory<ParcInformatiqueContext>
    {
        private readonly DbContextOptions<ParcInformatiqueContext> _options =
            new DbContextOptionsBuilder<ParcInformatiqueContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        public ParcInformatiqueContext CreateDbContext() => new(_options);
    }
}
