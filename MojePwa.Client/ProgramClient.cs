global using CT = System.Threading.CancellationToken;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MojePwa.Client;
using MojePwa.Client.Services.Browser;
using MojePwa.Client.Services.DataServices;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddRadzenComponents();


builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IFakeDataService, FakeDataService>();
builder.Services.AddScoped<BrowserStorage>();
builder.Services.AddScoped<BrowserTtlCache>();
builder.Services.AddScoped<BrowserStoredValueFactory>();

await builder.Build().RunAsync();
