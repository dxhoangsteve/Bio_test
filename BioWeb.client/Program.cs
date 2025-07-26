using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BioWeb.client;
using BioWeb.client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HttpClient to call server API
// Tự động detect môi trường và sử dụng URL phù hợp
var baseAddress = builder.HostEnvironment.IsProduction()
    ? builder.Configuration["ApiBaseUrl"] ?? "https://localhost:5001"  // Production: từ config hoặc default
    : "https://localhost:7254"; // Development: sử dụng port 7254

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(baseAddress) });

// Register services
builder.Services.AddScoped<IApiService, ApiService>();
builder.Services.AddScoped<BioWeb.client.Services.IAuthService, BioWeb.client.Services.AuthService>();

var app = builder.Build();

// Load auth token on startup
var apiService = app.Services.GetRequiredService<IApiService>();
await apiService.LoadAuthTokenAsync();

await app.RunAsync();
