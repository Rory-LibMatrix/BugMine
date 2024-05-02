using ArcaneLibs.Extensions;
using LibMatrix.RoomTypes;

namespace BugMine.Web.Classes;

public class BugMineProject(GenericRoom room) {
    public const string RoomType = "gay.rory.bugmine.project";
    public GenericRoom Room { get; } = room;
    public ProjectInfo Info { get; set; }
    public BugMineRoomMetadata Metadata { get; set; }
    public string ProjectSlug { get; set; }

    public async Task<BugMineProject> InitializeAsync() {
        var infoTask = Room.GetStateAsync<ProjectInfo>(ProjectInfo.EventId);
        var metadataTask = Room.GetStateAsync<BugMineRoomMetadata>(BugMineRoomMetadata.EventId)!;
        var alias = await room.GetCanonicalAliasAsync();

        if (alias != null)
            ProjectSlug = alias.Alias?[1..] ?? room.RoomId;
        else ProjectSlug = room.RoomId;

        Info = (await infoTask)!;
        Metadata = (await metadataTask)!;
        return this;
    }

    public async Task<BugMineIssue> CreateIssue(BugMineIssueData issue) {
        // add relation to room creation event
        issue.RelatesTo = new() {
            EventId = Metadata.RoomCreationEventId,
            RelationType = "gay.rory.bugmine.issue"
        };
        var eventId = await Room.SendTimelineEventAsync(BugMineIssueData.EventId, issue);

        var evt = await room.GetEventAsync(eventId.EventId);
        Console.WriteLine(evt.ToJson(indent: false));
        return new BugMineIssue(Room, evt);
    }

    public async IAsyncEnumerable<BugMineIssue> GetIssues() {
        await foreach (var evt in room.GetRelatedEventsAsync(Metadata.RoomCreationEventId, "gay.rory.bugmine.issue", BugMineIssueData.EventId)) {
            yield return new BugMineIssue(Room, evt);
        }
    }
}

public static class ProjectRoomExtensions {
    public static async Task<BugMineProject> AsBugMineProject(this GenericRoom room, SemaphoreSlim? semaphore = null) {
        try {
            await (semaphore?.WaitAsync() ?? Task.CompletedTask);
            return await new BugMineProject(room).InitializeAsync();
        }
        finally {
            semaphore?.Release();
        }
    }
}