using LibMatrix;
using LibMatrix.RoomTypes;

namespace BugMine.Web.Classes;

public class BugMineIssue(GenericRoom room, StateEventResponse data) {
    public GenericRoom Room => room ?? throw new ArgumentNullException(nameof(room));
    public StateEventResponse Data => data ?? throw new ArgumentNullException(nameof(data));
    // public async IAsyncEnumerable<StateEventResponse> GetRelatedEventsAsync() {
    //     
    // }
}