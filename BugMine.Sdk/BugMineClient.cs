using System.Text.RegularExpressions;
using ArcaneLibs.Extensions;
using LibMatrix.Homeservers;
using LibMatrix.Responses;

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
        var alias = string.Join('_', Regex.Matches(request.Name, @"[a-zA-Z0-9]+").Select(x => x.Value))+"-bugmine";
        
        var crr = new CreateRoomRequest() {
            CreationContent = new() {
                ["type"] = "gay.rory.bugmine.project"
            },
            Name = $"{request.Name} (BugMine project)",
            RoomAliasName = alias
        };
        
        var response = await Homeserver.CreateRoom(crr);
        await response.SendStateEventAsync(ProjectInfo.EventId, request);
        
        return await response.AsBugMineProject();
    }

    public async Task<BugMineProject?> GetProject(string projectSlug) {
        if (projectSlug.StartsWith('!')) {
            var room = homeserver.GetRoom(projectSlug);
            if (room == null) return null;

            return await (await room.AsBugMineProject()).InitializeAsync();
        }
        else {
            var alias = $"#{projectSlug}";
            var resolveResult = await Homeserver.ResolveRoomAliasAsync(alias);
            if (string.IsNullOrEmpty(resolveResult?.RoomId)) return null; //TODO: fallback to finding via joined rooms' canonical alias event?
            
            return await (await homeserver.GetRoom(resolveResult.RoomId).AsBugMineProject()).InitializeAsync();
        }
    }
}