using System.Text.Json.Serialization;
namespace GestionOrdinateurs.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EtatOrdinateur
{
    EnStock,
    EnService,
    EnMaintenance,
    EnPanne,
    HorsService,
    Lent,
    Reparable
}