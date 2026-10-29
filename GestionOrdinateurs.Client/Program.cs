using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using GestionOrdinateurs.Client;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using GestionOrdinateurs.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
var authenticationApiBaseUrl = builder.Configuration["AuthenticationApi:BaseUrl"]
    ?? throw new InvalidOperationException("AuthenticationApi:BaseUrl is missing.");
builder.Services.AddHttpClient("AuthenticationApi", client =>
    client.BaseAddress = new Uri(authenticationApiBaseUrl));
var gatewayBaseUrl = builder.Configuration["Gateway:BaseUrl"]
    ?? throw new InvalidOperationException("Gateway:BaseUrl is missing.");
builder.Services.AddHttpClient("Gateway", client =>
    client.BaseAddress = new Uri(gatewayBaseUrl));
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<SessionAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(services =>
    services.GetRequiredService<SessionAuthenticationStateProvider>());
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<GestionOrdinateurs.Client.Services.GatewayApiClient>();

await builder.Build().RunAsync();
