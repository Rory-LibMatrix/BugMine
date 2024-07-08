using System.Diagnostics;
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
                              2) Mass create projects
                              3) Destroy all projects
                              4) Get room count
                              5) Summarize all projects
                              6) Mass create regular rooms
                              7) Create issues in project7

                              L) Logout
                              Q) Quit
                              """);

            var input = Console.ReadKey();
            Console.WriteLine();
            switch (input.Key) {
                case ConsoleKey.D1: {
                    var sw = Stopwatch.StartNew();
                    var projects = Client.GetProjects();
                    await foreach (var project in projects) {
                        Console.WriteLine(project.ToJson(indent: false));
                    }

                    Console.WriteLine($"Queried in {sw.Elapsed}");
                    break;
                }
                case ConsoleKey.D2: {
                    await MassCreateProjects();
                    break;
                }
                case ConsoleKey.D3: {
                    await DestroyAllProjects();
                    break;
                }
                case ConsoleKey.D4: {
                    var sw = Stopwatch.StartNew();
                    var rooms = await Client.Homeserver.GetJoinedRooms();
                    Console.WriteLine(rooms.Count() + " rooms.");
                    var projectCount = 0;
                    await foreach (var room in Client.Homeserver.GetJoinedRoomsByType("gay.rory.bugmine.project").WithCancellation(stoppingToken)) {
                        projectCount++;
                    }

                    Console.WriteLine($"{projectCount} projects ({rooms.Count} rooms) queried in {sw.Elapsed}");
                    break;
                }
                case ConsoleKey.D5: {
                    await SummarizeAllProjects();
                    break;
                }
                case ConsoleKey.D6: {
                    var sw = Stopwatch.StartNew();
                    await Task.WhenAll(Enumerable.Range(0,1000).Select(async _ => {
                        await Client.Homeserver.CreateRoom(new() {
                            Name = Guid.NewGuid().ToString()[..8]
                        });
                    }));
                    Console.WriteLine($"Created 1000 rooms in {sw.Elapsed}");
                    break;
                }
                case ConsoleKey.D7: {
                    Console.Write("Slug: ");
                    var slug = Console.ReadLine();
                    var project = await Client.GetProject(slug);
                    if (project == null) {
                        Console.WriteLine("Project not found.");
                        break;
                    }
                    Console.Write("Count: ");
                    if (!int.TryParse(Console.ReadLine(), out var count)) {
                        Console.WriteLine("Invalid count.");
                        break;
                    }
                    await Task.WhenAll(Enumerable.Range(0,count).Select(async _ => {
                        await project.CreateIssue(new() {
                            Name = Guid.NewGuid().ToString()[..8]
                        });
                    }));
                    break;
                }
                case ConsoleKey.D8: {
                    await foreach (var board in Client.GetProjects(ignoreInvalidBoards: true).WithCancellation(stoppingToken)) {
                        await foreach (var issue in board.GetIssues().WithCancellation(stoppingToken)) {
                            Console.WriteLine(issue.Data.TypedContent);
                        }
                    }
                    break;
                }
                case ConsoleKey.L: {
                    File.Delete("auth.json");
                    await ExecuteAsync(stoppingToken);
                    return;
                }
                case ConsoleKey.Q: {
                    Environment.Exit(0);
                    return;
                }
            }
        }
    }

    private async Task DestroyAllProjects() {
        var ss = new SemaphoreSlim(48, 48);
        await foreach (var proj in Client.Homeserver.GetJoinedRoomsByType("gay.rory.bugmine.project")) {
            Task.Run(async () => {
                // await ss.WaitAsync();
                // await proj.SendStateEventAsync(RoomNameEventContent.EventId, new RoomNameEventContent() {
                //     Name = "Disbanded BugMine project."
                // });
                // await proj.SendStateEventAsync(RoomJoinRulesEventContent.EventId, new RoomJoinRulesEventContent() {
                //     JoinRule = RoomJoinRulesEventContent.JoinRules.Private
                // });
                // await proj.SendStateEventAsync(RoomCanonicalAliasEventContent.EventId, new RoomCanonicalAliasEventContent() {
                //     Alias = null
                // });
                await proj.LeaveAsync("[BugMine.DevTools.CLI] Disbanding project.");
                // ss.Release();
            });
        }
    }

    private async Task MassCreateProjects() {
        // var rooms = await Client.Homeserver.GetJoinedRooms();
        // List<string> roomNames = (await Task.WhenAll(rooms.Select(x => x.GetNameAsync()))).Where(x => x != 
        var tasks = new List<Task>();
        for (int i = 0; i < 500; i++) {
            tasks.Add(Task.Run(async () => {
                // var randomName = roomNames[Random.Shared.Next(roomNames.Count)];
                var proj = await Client.CreateProject(new() {
                    Name = /*randomName + */Guid.NewGuid().ToString()[..8]
                });

                // await proj.CreateIssue(new() {
                //     Name = "meow"
                // });
                await CreateRandomIssues(proj, Random.Shared.Next(20));
            }));
            // await Task.Delay(250);
        }

        await Task.WhenAll(tasks);
    }

    private async Task<string> SummarizeProject(BugMineProject project) {
        string result = $"Project: {project.Info.Name}, slug: {project.ProjectSlug}, metadata: {project.Metadata.ToJson(indent: false)}";
        await foreach (var issue in project.GetIssues(chunkLimit:50000)) {
            // Console.WriteLine($" - {issue.Data.RawContent.ToJson(indent: false)}");
            result += $"\n - {issue.Data.RawContent.ToJson(indent: false)}";
        }

        return result;
    }

    private async Task SummarizeAllProjects() {
        var sw = Stopwatch.StartNew();
        var tasks = new List<Task<string>>();
        await foreach (var project in Client.GetProjects()) {
            tasks.Add(SummarizeProject(project));
        }

        int projects = 0;
        int issues = 0;
        await foreach (var res in tasks.ToAsyncEnumerable()) {
            Console.WriteLine(res + "\n");
            projects++;
            issues += res.Count(x => x == '\n');
        }

        Console.WriteLine($"Summarized {projects} projects with {issues} issues in {sw.Elapsed}");
    }

    private async Task CreateRandomIssues(BugMineProject project, int count) {
        await Task.WhenAll(Enumerable.Range(0, count).Select(async i => {
            await project.CreateIssue(new() {
                Name = $"Issue {i} @ {DateTime.Now}"
            });
        }));
    }
}