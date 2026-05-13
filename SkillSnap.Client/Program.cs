using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client;
using SkillSnap.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5160/";
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SkillService>();
builder.Services.AddScoped<PortfolioUserService>();

await builder.Build().RunAsync();
