namespace BugMine.CLI.Interfaces;

public abstract class BaseTUIMenu(ILogger logger) {
    public virtual Dictionary<string, Func<Task>> MenuItems { get; set; }

    public virtual async Task Execute() { }

    public void HandleMenu() {
        if (MenuItems == null) {
            logger.LogCritical("Menu {type} had no MenuItems intialised!", this.GetType().FullName);
            Environment.Exit(1);
        }
        
        int currentIndex = 0;
        bool running = true;
        // int startHeight = Console.CursorTop;
        // Console.WriteLine("sh - " + startHeight);
        while (running) {
            Console.CursorLeft = 0;
            
            // Console.CursorTop = startHeight;
            Console.Beep();
            int i = 0;
            foreach (var (key, value) in MenuItems) {
                Console.Beep();
                Thread.Sleep(25);
                Console.WriteLine($"{(i++ == currentIndex ? ">" : " ")} {key}                        " + Console.CursorTop);
            }

            Console.CursorTop -= MenuItems.Count - currentIndex;
            var oldIndex = currentIndex;
            var ckey = Console.ReadKey(true);
            Console.Write(ckey.Key);
            switch (ckey.Key) {
                case ConsoleKey.DownArrow:
                    currentIndex++;
                    if (currentIndex >= MenuItems.Count()) currentIndex = MenuItems.Count() - 1;
                    Console.Write($"  NEXT ENTRY: {currentIndex}");
                    break;
                case ConsoleKey.UpArrow:
                    currentIndex--;
                    if (currentIndex < 0) currentIndex = 0;
                    Console.Write($"  NEXT ENTRY: {currentIndex}");
                    break;
            }

            // Console.CursorTop -= MenuItems.Count() - oldIndex;
            Thread.Sleep(250);
            // Console.CursorTop -= currentIndex - 1;
        }
    }
}