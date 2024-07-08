using LibMatrix.EventTypes;

namespace BugMine.Sdk.Events.State;

[MatrixEvent(EventName = EventId)]
public class BugMineRoomMetadata : EventContent {
    public const string EventId = "gay.rory.bugmine.room_metadata";
    
    public string RoomCreationEventId { get; set; }

}