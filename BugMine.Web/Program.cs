using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using BugMine.Web;
using BugMine.Web.Classes;
using LibMatrix.Interfaces.Services;
using LibMatrix.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
try {
    builder.Configuration.AddJsonStream(await new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }.GetStreamAsync("/appsettings.json"));
#if DEBUG
    builder.Configuration.AddJsonStream(await new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) }.GetStreamAsync("/appsettings.Development.json"));
#endif
}
catch (HttpRequestException e) {
    if (e.StatusCode == HttpStatusCode.NotFound)
        Console.WriteLine("Could not load appsettings, server returned 404.");
    else
        Console.WriteLine("Could not load appsettings: " + e);
}
catch (Exception e) {
    Console.WriteLine("Could not load appsettings: " + e);
}

builder.Logging.AddConfiguration(
    builder.Configuration.GetSection("Logging"));

builder.Services.AddBlazoredLocalStorage(config => {
    config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    config.JsonSerializerOptions.WriteIndented = false;
});

builder.Services.AddBlazoredSessionStorage(config => {
    config.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    config.JsonSerializerOptions.IgnoreReadOnlyProperties = true;
    config.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    config.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    config.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
    config.JsonSerializerOptions.WriteIndented = false;
});

// builder.Services.AddRoryLibMatrixServices();
builder.Services.AddBugMine();
builder.Services.AddScoped<IStorageProvider, LocalStorageProviderService>();
builder.Services.AddScoped<BugMineStorage>();

await builder.Build().RunAsync();