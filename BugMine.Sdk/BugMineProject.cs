using System.Text.Json.Nodes;
using LibMatrix.EventTypes.Spec.State;
using LibMatrix.Homeservers;
using LibMatrix.RoomTypes;

namespace BugMine.Web.Classes;

public class BugMineProject(GenericRoom room) {
    public const string RoomType = "gay.rory.bugmine.project";
    public GenericRoom Room { get; } = room;
    public ProjectInfo Info { get; set; }
    public string ProjectSlug { get; set; }

    public async Task<BugMineProject> InitializeAsync() {
        Info = (await Room.GetStateAsync<ProjectInfo>(ProjectInfo.EventId))!;
        var alias = await room.GetCanonicalAliasAsync();

        if (alias != null)
            ProjectSlug = alias.Alias?[1..] ?? room.RoomId;
        else ProjectSlug = room.RoomId;

        return this;
    }

    public async Task<BugMineIssue> CreateIssue(BugMineIssueData issue) {
        // add relation to room creation event
        issue.RelatesTo = new() {
            EventId = await room.GetStateEventIdAsync(RoomCreateEventContent.EventId),
            RelationType = "gay.rory.bugmine.issue"
        };
        var eventId = await Room.SendTimelineEventAsync(BugMineIssueData.EventId, issue);

        // return new BugMineIssueAccessor(Room, await Room.GetEventAsync<>(eventId));
        var evt = await room.GetEventAsync(eventId.EventId);
        Console.WriteLine(evt);
        return new BugMineIssue(Room, evt);
    }

    public async IAsyncEnumerable<BugMineIssue> GetIssues() {
        var creationEventId = await room.GetStateEventIdAsync(RoomCreateEventContent.EventId);
        await foreach (var evt in room.GetRelatedEventsAsync(creationEventId, "gay.rory.bugmine.issue", BugMineIssueData.EventId)) {
            yield return new BugMineIssue(Room, evt);
        }
    }
}

public static class ProjectRoomExtensions {
    public static async Task<BugMineProject> AsBugMineProject(this GenericRoom room) {
        return await new BugMineProject(room).InitializeAsync();
    }
}