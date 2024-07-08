using LibMatrix.EventTypes;

namespace BugMine.Sdk.Events.State;

[MatrixEvent(EventName = EventId)]
public class ProjectInfo : EventContent {
    public const string EventId = "gay.rory.bugmine.project_info";
    public string? Name { get; set; }
    public string? ProjectIcon { get; set; }
    public string? Repository { get; set; }
}