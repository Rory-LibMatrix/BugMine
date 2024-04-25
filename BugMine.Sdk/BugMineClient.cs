using System.Text.RegularExpressions;
using ArcaneLibs.Extensions;
using BugMine.Web.Classes.Exceptions;
using LibMatrix;
using LibMatrix.Homeservers;
using LibMatrix.Responses;
using LibMatrix.RoomTypes;

namespace BugMine.Web.Classes;

public class BugMineClient(AuthenticatedHomeserverGeneric homeserver) {
    public AuthenticatedHomeserverGeneric Homeserver { get; } = homeserver;

    public async IAsyncEnumerable<BugMineProject> GetProjects() {
        List<Task<BugMineProject>> tasks = [];
        await foreach (var room in homeserver.GetJoinedRoomsByType(BugMineProject.RoomType)) {
            tasks.Add(room.AsBugMineProject());
        }

        var results = tasks.ToAsyncEnumerable();
        await foreach (var result in results) {
            yield return result;
        }
    }

    public async Task<BugMineProject> CreateProject(ProjectInfo request) {
        var alias = string.Join('_', Regex.Matches(request.Name, @"[a-zA-Z0-9]+").Select(x => x.Value)) + "-bugmine";

        var crr = new CreateRoomRequest() {
            CreationContent = new() {
                ["type"] = "gay.rory.bugmine.project"
            },
            Name = $"{request.Name} (BugMine project)",
            RoomAliasName = alias,
            InitialState = [
                new StateEvent() {
                    Type = "m.room.join_rules",
                    RawContent = new() {
                        ["join_rule"] = "public"
                    }
                },
                new StateEvent() {
                    Type = ProjectInfo.EventId,
                    TypedContent = request
                }
            ]
        };

        var response = await Homeserver.CreateRoom(crr);
        // await response.SendStateEventAsync(ProjectInfo.EventId, request);

        return await response.AsBugMineProject();
    }

    public async Task<BugMineProject?> GetProject(string projectSlug) {
        var room = await ResolveProjectSlug(projectSlug);

        if (room == null) return null;

        var rooms = await Homeserver.GetJoinedRooms();
        if (!rooms.Any(r => r.RoomId == room.RoomId)) throw new BugMineException(BugMineException.ErrorCodes.UserNotInRoom);

        return await (await room.AsBugMineProject()).InitializeAsync();
    }

    public async Task<GenericRoom?> ResolveProjectSlug(string projectSlug) {
        GenericRoom? room;
        if (projectSlug.StartsWith('!')) {
            room = homeserver.GetRoom(projectSlug);
        }
        else {
            var alias = $"#{projectSlug}";
            var resolveResult = await Homeserver.ResolveRoomAliasAsync(alias);
            if (string.IsNullOrEmpty(resolveResult?.RoomId)) return null; //TODO: fallback to finding via joined rooms' canonical alias event?

            room = homeserver.GetRoom(resolveResult.RoomId);
        }

        return room;
    }
}