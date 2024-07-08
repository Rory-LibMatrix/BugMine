using BugMine.Sdk.Events.State;
using BugMine.Web.Classes;
using LibMatrix.EventTypes;

namespace BugMine.Sdk.Events.Timeline;

[MatrixEvent(EventName = EventId)]
public class BugMineIssueData : TimelineEventContent {
    public const string EventId = "gay.rory.bugmine.issue";
    public string Name { get; set; }
    public string? AssignedTo { get; set; }
    public string? Status { get; set; }
    public string? Priority { get; set; }
}