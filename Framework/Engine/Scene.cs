namespace Framework.Engine;

public abstract class Scene
{
    private readonly List<GameObject> _gameObjects = [];

    private Action<Scene>? _requestSceneChange;
    private bool _hasEntered;
    private bool _isActive;

    public Camera2D Camera { get; } = new();

    internal IReadOnlyList<GameObject> GameObjects => _gameObjects;

    protected GameObject CreateGameObject(
        string name = "GameObject")
    {
        GameObject gameObject = new(name);
        _gameObjects.Add(gameObject);

        return gameObject;
    }

    protected bool RemoveGameObject(GameObject gameObject)
    {
        ArgumentNullException.ThrowIfNull(gameObject);

        int index = _gameObjects.FindIndex(
            existing => ReferenceEquals(existing, gameObject));

        if (index < 0)
        {
            return false;
        }

        _gameObjects.RemoveAt(index);
        return true;
    }

    protected void ChangeScene(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        Action<Scene> requestSceneChange =
            _requestSceneChange ??
            throw new InvalidOperationException(
                "Scene changes can only be requested by an active scene.");

        requestSceneChange(scene);
    }

    protected virtual void OnEnter()
    {
    }

    protected virtual void Update(float deltaTime)
    {
    }

    protected virtual void OnExit()
    {
    }

    internal void Enter(Action<Scene> requestSceneChange)
    {
        ArgumentNullException.ThrowIfNull(requestSceneChange);

        if (_hasEntered)
        {
            throw new InvalidOperationException(
                $"Scene '{GetType().Name}' has already been activated. " +
                "Scene instances can only be activated once.");
        }

        _hasEntered = true;
        _isActive = true;
        _requestSceneChange = requestSceneChange;

        OnEnter();
    }

    internal void UpdateInternal(float deltaTime)
    {
        if (!_isActive)
        {
            throw new InvalidOperationException(
                $"Scene '{GetType().Name}' is not active.");
        }

        Update(deltaTime);
    }

    internal void Exit()
    {
        if (!_isActive) return;

        _isActive = false;
        _requestSceneChange = null;

        OnExit();
    }
}
