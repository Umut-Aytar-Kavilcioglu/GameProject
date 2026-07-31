namespace Framework.Engine;

public sealed class GameObject
{
    private readonly Dictionary<Type, Component> _components = [];

    public GameObject(string name = "GameObject")
    {
        Name = name;
    }

    public string Name { get; set; }
    public bool IsActive { get; set; } = true;

    public T AddComponent<T>(T component)
        where T : Component
    {
        ArgumentNullException.ThrowIfNull(component);

        Type componentType = component.GetType();

        component.AttachTo(this);

        try
        {
            if (!_components.TryAdd(componentType, component))
            {
                throw new InvalidOperationException(
                    $"GameObject '{Name}' already contains a component " +
                    $"of type '{componentType.Name}'.");
            }
        }
        catch
        {
            component.Detach();
            throw;
        }

        return component;
    }

    public bool RemoveComponent(Component component)
    {
        ArgumentNullException.ThrowIfNull(component);

        Type componentType = component.GetType();

        if (!_components.TryGetValue(componentType, out Component? existingComponent) ||
            !ReferenceEquals(existingComponent, component))
        {
            return false;
        }

        _components.Remove(componentType);
        component.Detach();

        return true;
    }

    public T? GetComponent<T>()
        where T : Component
    {
        if (_components.TryGetValue(typeof(T), out Component? component))
        {
            return (T)component;
        }

        return null;
    }
}
