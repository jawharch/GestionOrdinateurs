using System.Text.Json.Serialization;

namespace GestionOrdinateurs.Shared;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TypeOrdinateur
{
    Portable,
    Fixe,
    Server
}
