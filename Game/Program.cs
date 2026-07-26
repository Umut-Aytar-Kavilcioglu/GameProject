using Framework;

namespace Game;

internal static class Program
{
    private static void Main()
    {
        using Engine engine = new(title: "Game", width: 1280, height: 720);
        engine.Run();
    }
}
