
namespace Framework.Engine;

public abstract class Component
{
    public GameObject? GameObject { get; private set; }
    public bool IsActive { get; set; } = true;
    public bool IsAttached => GameObject is not null;

    internal void AttachTo(GameObject gameObject)
    {
        ArgumentNullException.ThrowIfNull(gameObject);

        if (GameObject is not null)
        {
            throw new InvalidOperationException(
                $"Component '{GetType().Name}' is already attached to " +
                $"GameObject '{GameObject.Name}'.");
        }

        GameObject = gameObject;
    }

    internal void Detach()
    {
        if (GameObject is null) return;
        GameObject = null;
    }
}
