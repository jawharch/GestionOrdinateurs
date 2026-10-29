using System.ComponentModel.DataAnnotations.Schema;

namespace GestionOrdinateurs.Shared;

public class Caracteristiques
{
    public int RamGo { get; set; }
    public TypeOrdinateur TypeOrdi { get; set; }
    public int StockageGo { get; set; }
    public string Processeur { get; set; } = string.Empty;
    public double TailleEcran { get; set; }
    public int AutonomieHeures { get; set; }
    public int PuissanceW { get; set; }
    public bool CarteGraphique { get; set; }

    [NotMapped]
    public MemoireOrdinateur Memoire => new(RamGo, StockageGo);

    public Caracteristiques()
    {
    }

    public Caracteristiques(
        int ramGo,
        int stockageGo,
        TypeOrdinateur typeOrdi,
        string processeur = "",
        double tailleEcran = 0,
        int autonomieHeures = 0,
        int puissanceW = 0,
        bool carteGraphique = false)
    {
        RamGo = ramGo;
        StockageGo = stockageGo;
        TypeOrdi = typeOrdi;
        Processeur = processeur;
        TailleEcran = tailleEcran;
        AutonomieHeures = autonomieHeures;
        PuissanceW = puissanceW;
        CarteGraphique = carteGraphique;
    }
}
