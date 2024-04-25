using System.Text.Json;
using ArcaneLibs.Extensions;
using BugMine.Web.Classes;
using LibMatrix.EventTypes.Spec.State;
using LibMatrix.Responses;
using LibMatrix.Services;

namespace BugMine.DevTools.CLI;

public class Worker(ILogger<Worker> logger, HomeserverProviderService hsProvider) : BackgroundService {
    private readonly ILogger<Worker> _logger = logger;
    private BugMineClient? Client { get; set; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        if (!File.Exists("auth.json")) {
            Console.Write("Homeserver: ");
            var homeserver = Console.ReadLine()!;
            Console.Write("Username: ");
            var username = Console.ReadLine()!;
            Console.Write("Password: ");
            var password = Console.ReadLine()!;

            var login = await hsProvider.Login(homeserver, username, password);
            await File.WriteAllTextAsync("auth.json", login.ToJson(), stoppingToken);
        }

        var auth = await JsonSerializer.DeserializeAsync<LoginResponse>(File.OpenRead("auth.json"));
        Client = new BugMineClient(await hsProvider.GetAuthenticatedWithToken(auth.Homeserver, auth.AccessToken, useGeneric: true));

        while (!stoppingToken.IsCancellationRequested) {
            Console.WriteLine("""
                              1) List all projects
                              2) Create 500 projects
                              3) Destroy all projects
                              4) Get room count
                              """);

            var input = Console.ReadKey();
            Console.WriteLine();
            switch (input.Key) {
                case ConsoleKey.D1: {
                    if (Client is null) {
                        _logger.LogError("No client available.");
                        break;
                    }

                    var projects = Client.GetProjects();
                    await foreach (var project in projects) {
                        Console.WriteLine(project.ToJson(indent: false));
                    }

                    break;
                }
                case ConsoleKey.D2: {
                    if (Client is null) {
                        _logger.LogError("No client available.");
                        break;
                    }

                    await MassCreateProjects();
                    break;
                }
                case ConsoleKey.D3: {
                    if (Client is null) {
                        _logger.LogError("No client available.");
                        break;
                    }

                    await DestroyAllProjects();
                    break;
                }
                case ConsoleKey.D4: {
                    if (Client is null) {
                        _logger.LogError("No client available.");
                        break;
                    }

                    var rooms = await Client.Homeserver.GetJoinedRooms();
                    Console.WriteLine(rooms.Count());
                    break;
                }
            }
        }
    }

    private async Task DestroyAllProjects() {
        var ss = new SemaphoreSlim(4, 4);
        await foreach (var proj in Client.Homeserver.GetJoinedRoomsByType(null!)) {
            if (proj.RoomId == "!UktPWOzit8gmms5FQ6:conduit.matrixunittests.rory.gay") continue;
            Task.Run(async () => {
                // await ss.WaitAsync();
                await proj.SendStateEventAsync(RoomNameEventContent.EventId, new RoomNameEventContent() {
                    Name = "Disbanded BugMine project."
                });
                await proj.SendStateEventAsync(RoomJoinRulesEventContent.EventId, new RoomJoinRulesEventContent() {
                    JoinRule = RoomJoinRulesEventContent.JoinRules.Private
                });
                await proj.SendStateEventAsync(RoomCanonicalAliasEventContent.EventId, new RoomCanonicalAliasEventContent() {
                    Alias = null
                });
                await proj.LeaveAsync("Disbanded room.");
                // ss.Release();
            });
        }
    }

    private async Task MassCreateProjects() {
        // var rooms = await Client.Homeserver.GetJoinedRooms();
        // List<string> roomNames = (await Task.WhenAll(rooms.Select(x => x.GetNameAsync()))).Where(x => x != null).ToList();
        for (int i = 0; i < 500; i++) {
            Task.Run(async () => {
                // var randomName = roomNames[Random.Shared.Next(roomNames.Count)];
                var proj = await Client.CreateProject(new() {
                    Name = /*randomName + */Guid.NewGuid().ToString()[..8]
                });

                // await proj.CreateIssue(new() {
                // Name = "meow"
                // });
            });
        }
    }
}