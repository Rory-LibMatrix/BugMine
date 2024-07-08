using LibMatrix.EventTypes;

namespace BugMine.Sdk.Events.Timeline;

[MatrixEvent(EventName = EventId)]
public class BugMineIssueComment : TimelineEventContent {
    public const string EventId = "gay.rory.bugmine.issue.comment";
    public string Comment { get; set; }
    public string Author { get; set; }
    public DateTime Timestamp { get; set; }
}