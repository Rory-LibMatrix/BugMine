using LibMatrix.EventTypes;

namespace BugMine.Web.Classes;

[MatrixEvent(EventName = ProjectInfo.EventId)]
public class BugMineIssueData : TimelineEventContent {
    public const string EventId = "gay.rory.bugmine.issue";
    public string Name { get; set; }
    public string? AssignedTo { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
}