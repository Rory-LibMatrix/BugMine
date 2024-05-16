using System.Diagnostics.CodeAnalysis;
using LibMatrix;

namespace BugMine.Web.Classes.Exceptions;

public class BugMineException : MatrixException {
    [SetsRequiredMembers]
    public BugMineException(string errorCode, string? message = null) {
        ErrorCode = errorCode;
        Error = message ?? Message;
    }

    public sealed override string Message =>
        $"{ErrorCode}: {ErrorCode switch {
            // common
            ErrorCodes.UserNotInRoom => "User is not in the room",
            _ => base.Message
        }}";

    public new static class ErrorCodes {
        public const string UserNotInRoom = "BUGMINE_USER_NOT_IN_ROOM";
        public const string ProjectNotFound = "BUGMINE_PROJECT_NOT_FOUND";
    }
}