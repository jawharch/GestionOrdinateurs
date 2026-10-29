namespace GestionOrdinateurs.Shared;

public class InventaireOrdinateurs
{
    public List<Ordinateur> Ordinateurs { get; set; } = [];

    public Ordinateur? this[string reference] =>
        Ordinateurs.FirstOrDefault(ordinateur =>
            string.Equals(ordinateur.Nom, reference, StringComparison.OrdinalIgnoreCase));

    public Ordinateur this[int index] => Ordinateurs[index];
}
