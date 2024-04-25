using LibMatrix.EventTypes;

namespace BugMine.Web.Classes;

[MatrixEvent(EventName = EventId)]
public class BugMineRoomMetadata : EventContent {
    public const string EventId = "gay.rory.bugmine.room_metadata";
    
    public string RoomCreationEventId { get; set; }
    
}