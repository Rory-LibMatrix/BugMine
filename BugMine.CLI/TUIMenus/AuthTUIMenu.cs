using BugMine.CLI.Interfaces;

namespace BugMine.CLI.TUIMenus;

public class AuthTUIMenu(ILogger<AuthTUIMenu> logger) : BaseTUIMenu(logger) {
    public override async Task Execute() {
        Console.WriteLine("meow from AuthTUIMenu");
        HandleMenu();
        await Task.Delay(10000);
    }
}