using System.Numerics;
using Framework.Engine;

namespace Game;

internal sealed class RendererTestScene : Scene
{
    private readonly Texture2D _texture;

    public RendererTestScene(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        _texture = texture;
    }

    protected override void OnEnter()
    {
        const int spriteCount = 5_000;
        const int logicalWidth = 480;
        const int logicalHeight = 270;

        for (int index = 0; index < spriteCount; index++)
        {
            GameObject gameObject = CreateGameObject();

            gameObject.AddComponent(new Transform
            {
                Position = new Vector2(
                    x: (index * 37) % logicalWidth,
                    y: (index * 17) % logicalHeight)
            });

            gameObject.AddComponent(
                new Sprite2D(_texture));
        }
    }
}
