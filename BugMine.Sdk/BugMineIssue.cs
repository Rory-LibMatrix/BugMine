using LibMatrix;
using LibMatrix.RoomTypes;

namespace BugMine.Web.Classes;

public class BugMineIssue(GenericRoom room, StateEventResponse data) {
    public GenericRoom Room { get; } = room;
    public StateEventResponse Data { get; } = data;
    // public async IAsyncEnumerable<StateEventResponse> GetRelatedEventsAsync() {
    //     
    // }
}