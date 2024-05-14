using System.Text.Json;
using ArcaneLibs.Extensions;
using BugMine.CLI.Interfaces;
using BugMine.CLI.TUIMenus;
using BugMine.Web.Classes;
using LibMatrix.Responses;
using LibMatrix.Services;

namespace BugMine.CLI;

public class InteractiveClient(ILogger<InteractiveClient> logger, BugMineClient client, MainTUIMenu mainMenu) : BackgroundService {
    private readonly ILogger<InteractiveClient> _logger = logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        while (!stoppingToken.IsCancellationRequested) {
            //             Console.WriteLine("""
            //                               1) List all projects
            //                               2) Mass create projects
            //                               3) Destroy all projects
            //                               4) Get room count
            //                               5) Summarize all projects
            //                               6) Mass create regular rooms
            //
            //                               L) Logout
            //                               Q) Quit
            //                               """);
            //             
            //             var input = Console.ReadKey();
            //             Console.WriteLine();
            //             switch (input.Key) {
            //                 
            //                 case ConsoleKey.L: {
            //                     File.Delete("auth.json");
            //                     await ExecuteAsync(stoppingToken);
            //                     return;
            //                 }
            //                 case ConsoleKey.Q: {
            //                     Environment.Exit(0);
            //                     return;
            //                 }
            //             }
            await mainMenu.Execute();
        }
    }
}