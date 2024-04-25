using BugMine.DevTools.CLI;
using BugMine.Web.Classes;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddBugMine(new() { AppName = "BugMine DevTools CLI" });
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();