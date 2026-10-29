using System.Net.Http.Headers;
using System.Net.Http.Json;
using Blazored.SessionStorage;

namespace GestionOrdinateurs.Client.Services;

public sealed class GatewayApiClient(
    IHttpClientFactory httpClientFactory,
    ISessionStorageService sessionStorage)
{
    public Task<IReadOnlyList<OrdinateurListeItem>?> GetOrdinateursAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<OrdinateurListeItem>>(
            "api/ordinateurs/liste",
            cancellationToken);

    public Task<IReadOnlyList<HistoriqueReparationItem>?> GetHistoriqueReparationsAsync(
        CancellationToken cancellationToken = default) =>
        GetAsync<IReadOnlyList<HistoriqueReparationItem>>(
            "api/reparations/historique",
            cancellationToken);

    private async Task<T?> GetAsync<T>(string path, CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Gateway");
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        var token = await sessionStorage.GetItemAsync<string>("authToken");
        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        using var response = await client.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
    }
}

public sealed record OrdinateurListeItem(
    int Id,
    string Nom,
    decimal Prix,
    string Devise,
    bool Vendu);

public sealed record HistoriqueReparationItem(
    int OrdinateurId,
    string NomOrdinateur,
    string Technicien,
    string TypeReparation,
    DateTime DateIntervention);
