using BugMine.CLI.Interfaces;

namespace BugMine.CLI.TUIMenus;

public class MainTUIMenu(ILogger<MainTUIMenu> logger, AuthTUIMenu authMenu) : BaseTUIMenu(logger) {
    public override Dictionary<string, Func<Task>> MenuItems { get; set; } = new() {
        { "auth", () => authMenu.Execute() },
        { "1", () => Task.CompletedTask },
        { "2", () => Task.CompletedTask },
        { "3", () => Task.CompletedTask },
        { "4", () => Task.CompletedTask },
        { "5", () => Task.CompletedTask },
        { "6", () => Task.CompletedTask },
        { "L", () => Task.CompletedTask },
        { "Q", () => Task.CompletedTask }
    };

    public override async Task Execute() {
        Console.WriteLine("meow from MainTUIMenu");
        HandleMenu();
        await Task.Delay(10000);
    }
}