using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Client;
using Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdService>();
builder.Services.AddScoped<NotificationService>();

builder.Services.AddScoped(sp =>
{
    var auth = sp.GetRequiredService<AuthService>();
    var client = new HttpClient { BaseAddress = new Uri("http://localhost:5287") };
    return client;
});

await builder.Build().RunAsync();
