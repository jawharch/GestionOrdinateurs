using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Services.Events;

public sealed class OrdinateurAjouteEventArgs(Ordinateur ordinateur) : EventArgs
{
    public Ordinateur Ordinateur { get; } = ordinateur;
}
