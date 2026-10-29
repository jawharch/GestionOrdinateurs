using GestionOrdinateurs.Shared;

namespace GestionOrdinateurs.API.Services.Events;

public sealed class OrdinateurSupprimeEventArgs(Ordinateur ordinateur) : EventArgs
{
    public Ordinateur Ordinateur { get; } = ordinateur;
}
