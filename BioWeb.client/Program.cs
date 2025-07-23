using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BioWeb.client;
using BioWeb.client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to call server API
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7254") });

// Register services
builder.Services.AddScoped<IApiService, ApiService>();

await builder.Build().RunAsync();
