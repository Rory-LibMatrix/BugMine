namespace BugMine.Web.Classes;

public static class Constants {
#if DEBUG
    public const bool Debug = true;
#else
    public const bool Debug = false;
#endif
}