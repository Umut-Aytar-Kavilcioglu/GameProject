using Framework.Engine;

namespace Game;

internal static class Program
{
    private static void Main()
    {
        using Engine engine = new(
            title: "Game",
            width: 1280,
            height: 720,
            resizable: false,
            logicalWidth: 480,
            logicalHeight: 270,
            vsync: false);

        engine.Initialize();

        string testTexturePath = Path.Combine(
            AppContext.BaseDirectory,
            "Assets",
            "test.png");

        Texture2D testTexture = engine.Content.LoadTexture(
            name: "test",
            path: testTexturePath);

        engine.ChangeScene(
            new RendererTestScene(testTexture));

        engine.Run();
    }
}
