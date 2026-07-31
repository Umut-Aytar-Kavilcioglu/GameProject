namespace Framework.Engine;

public sealed class Sprite2D : Component
{
    public Sprite2D(Texture2D texture)
    {
        ArgumentNullException.ThrowIfNull(texture);
        Texture = texture;
    }

    public Texture2D Texture { get; }
}
