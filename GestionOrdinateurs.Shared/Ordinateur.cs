using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace GestionOrdinateurs.Shared;

[Table("Ordinateur")]
public class Ordinateur
{
    public int Id { get; set; }
    public string Nom { get; set; } = string.Empty;
    public string Marque { get; set; } = string.Empty;
    public EtatOrdinateur Etat { get; set; }

    [DefaultValue(0)]
    public int Stock { get; set; }

    [DefaultValue(0)]
    public decimal Prix { get; set; }

    public bool Vendu { get; set; }

    public Caracteristiques Caracteristiques { get; set; } = new();

    [JsonIgnore]
    public List<Reparation> Reparations { get; set; } = [];

    public Ordinateur()
    {
    }

    public Ordinateur(
        int id,
        string nom,
        string marque,
        Caracteristiques caracteristiques,
        EtatOrdinateur etat,
        decimal prix = 0,
        int stock = 0,
        bool vendu = false)
    {
        Id = id;
        Nom = nom;
        Marque = marque;
        Caracteristiques = caracteristiques;
        Etat = etat;
        Prix = prix;
        Stock = stock;
        Vendu = vendu;
    }

    public void ChangerEtat(EtatOrdinateur etat) => Etat = etat;

    public string Description() =>
        $"{Marque} {Nom} - {Caracteristiques.RamGo} Go - {Etat}";

    public void Modifier(
        string nom,
        string marque,
        Caracteristiques caracteristiques,
        EtatOrdinateur etat,
        decimal prix,
        int stock,
        bool vendu)
    {
        Nom = nom;
        Marque = marque;
        Caracteristiques = caracteristiques;
        Etat = etat;
        Prix = prix;
        Stock = stock;
        Vendu = vendu;
    }
}
