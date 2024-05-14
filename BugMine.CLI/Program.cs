using System.Text.Json;
using ArcaneLibs;
using ArcaneLibs.Extensions;
using BugMine.CLI;
using BugMine.CLI.Interfaces;
using BugMine.CLI.TUIMenus;
using BugMine.Web.Classes;using LibMatrix.Responses;
using LibMatrix.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddBugMine(new() { AppName = "BugMine CLI" });

builder.Services.AddSingleton<BugMineClient>(a => {
    var hsp = a.GetRequiredService<HomeserverProviderService>();
    return getClient(hsp, args.Length == 0).GetAwaiter().GetResult(); // We can't use async here, so we have to block
});

if (args.Length == 0) {
    Console.WriteLine("Starting interactive client...");
    builder.Services.AddHostedService<InteractiveClient>();
    // builder.Services.AddSingleton<BaseTUIMenu, MainTUIMenu>();
    var menus = new ClassCollector<BaseTUIMenu>().ResolveFromAllAccessibleAssemblies();
    foreach (var menu in menus) {
        Console.WriteLine($"Adding menu {menu.Name}");
        builder.Services.AddSingleton(menu);
        builder.Services.AddSingleton(typeof(BaseTUIMenu), menu);
    }
}
else {
    Console.WriteLine($"Starting CLI client with {args.Length} args: {string.Join(",", args)}");
    builder.Services.AddHostedService<CLIClient>();
}

var host = builder.Build();
host.Run();


async Task<LoginResponse> findAuth(HomeserverProviderService hsProvider, bool interactive = true) {
    Console.WriteLine($"findAuth entered with hsProvider={{{hsProvider.GetHashCode()}}}, interactive={interactive}");
    if (File.Exists("auth.json")) {
        return JsonSerializer.Deserialize<LoginResponse>(File.ReadAllText("auth.json"));
    }
    else {
        if (!interactive) {
            Console.WriteLine("Could not locate account information. Please log in interactively or use `BugMine.CLI login <mxid> <password>`.");
            Environment.Exit(1);
        }
        Console.Write("Homeserver: ");
        var homeserver = Console.ReadLine()!;
        Console.Write("Username: ");
        var username = Console.ReadLine()!;
        Console.Write("Password: ");
        var password = Console.ReadLine()!;

        var login = hsProvider.Login(homeserver, username, password).GetAwaiter().GetResult();
        File.WriteAllText("auth.json", login.ToJson());
        return login;
    }
}

async Task<BugMineClient> getClient(HomeserverProviderService hsProvider, bool interactive) {
    var auth = await findAuth(hsProvider, interactive);
    return new BugMineClient(await hsProvider.GetAuthenticatedWithToken(auth.Homeserver, auth.AccessToken, useGeneric: true));
}