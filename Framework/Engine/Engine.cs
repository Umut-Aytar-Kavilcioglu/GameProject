using System.Diagnostics;
using static Framework.SDL3.SDL3;

namespace Framework.Engine;

public sealed class Engine : IDisposable
{
    private readonly string _title;
    private readonly int _width;
    private readonly int _height;
    private readonly bool _resizable;
    private readonly int _logicalWidth;
    private readonly int _logicalHeight;
    private readonly bool _vsync;

    private IntPtr _window;

    private Renderer2D? _renderer;
    private ContentManager? _content;

    private Scene? _activeScene;
    private Scene? _pendingScene;

    private bool _isRunning;
    private bool _isInitialized;
    private bool _hasRun;
    private bool _isDisposed;

    public Engine(
        string title,
        int width,
        int height,
        bool resizable = false,
        int logicalWidth = 480,
        int logicalHeight = 270,
        bool vsync = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(logicalWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(logicalHeight);

        _title = title;
        _width = width;
        _height = height;
        _resizable = resizable;
        _logicalWidth = logicalWidth;
        _logicalHeight = logicalHeight;
        _vsync = vsync;
    }

    public ContentManager Content
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);

            return _content ??
                throw new InvalidOperationException(
                    "The engine must be initialized before accessing content.");
        }
    }

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_isInitialized)
        {
            return;
        }

        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            throw new InvalidOperationException(
                $"SDL initialization failed: {SDL_GetError()}");
        }

        try
        {
            SDL_WindowFlags windowFlags =
                SDL_WindowFlags.SDL_WINDOW_HIGH_PIXEL_DENSITY;

            if (_resizable)
            {
                windowFlags |=
                    SDL_WindowFlags.SDL_WINDOW_RESIZABLE;
            }

            _window = SDL_CreateWindow(
                _title,
                _width,
                _height,
                windowFlags);

            if (_window == IntPtr.Zero)
            {
                throw new InvalidOperationException(
                    $"Failed to create SDL window: {SDL_GetError()}");
            }

            _renderer = new Renderer2D(
                _window,
                _logicalWidth,
                _logicalHeight,
                _vsync);

            _content = new ContentManager(_renderer.Handle);
            _isInitialized = true;

            Console.WriteLine(
                $"Video driver: {SDL_GetCurrentVideoDriver()}");
        }
        catch
        {
            _content?.Dispose();
            _content = null;

            _renderer?.Dispose();
            _renderer = null;

            if (_window != IntPtr.Zero)
            {
                SDL_DestroyWindow(_window);
                _window = IntPtr.Zero;
            }

            SDL_Quit();
            throw;
        }
    }

    public void ChangeScene(Scene scene)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(scene);

        _pendingScene = scene;
    }

    public void Run()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_hasRun)
        {
            throw new InvalidOperationException(
                "An Engine instance can only be run once.");
        }

        Initialize();

        if (_activeScene is null && _pendingScene is null)
        {
            throw new InvalidOperationException(
                "A scene must be selected before running the engine.");
        }

        _hasRun = true;
        _isRunning = true;

        ApplyPendingScene();

        Stopwatch stopwatch = Stopwatch.StartNew();
        double previousTime = stopwatch.Elapsed.TotalSeconds;

        double fpsWindowStart = previousTime;
        int renderedFrames = 0;

        try
        {
            while (_isRunning)
            {
                ProcessInput();

                if (!_isRunning)
                {
                    break;
                }

                if (ApplyPendingScene())
                {
                    // Scene initialization time should not become delta time.
                    previousTime = stopwatch.Elapsed.TotalSeconds;
                }

                double currentTime = stopwatch.Elapsed.TotalSeconds;
                float deltaTime = (float)Math.Min(
                    currentTime - previousTime,
                    0.25);

                previousTime = currentTime;

                Scene activeScene =
                    _activeScene ??
                    throw new InvalidOperationException(
                        "The engine has no active scene.");

                activeScene.UpdateInternal(deltaTime);

                Renderer2D renderer =
                    _renderer ??
                    throw new InvalidOperationException(
                        "The renderer is not initialized.");

                renderer.Render(activeScene);

                renderedFrames++;

                double fpsCurrentTime = stopwatch.Elapsed.TotalSeconds;
                double fpsElapsedTime = fpsCurrentTime - fpsWindowStart;

                if (fpsElapsedTime >= 1.0)
                {
                    double framesPerSecond =
                        renderedFrames / fpsElapsedTime;

                    double averageFrameTimeMilliseconds =
                        fpsElapsedTime / renderedFrames * 1000.0;

                    Console.WriteLine(
                        $"FPS: {framesPerSecond:F1} | " +
                        $"Frame: {averageFrameTimeMilliseconds:F2} ms");

                    renderedFrames = 0;
                    fpsWindowStart = fpsCurrentTime;
                }
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    private void ProcessInput()
    {
        while (SDL_PollEvent(out SDL_Event @event))
        {
            SDL_EventType eventType =
                (SDL_EventType)@event.type;

            if (eventType is
                SDL_EventType.SDL_EVENT_QUIT or
                SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED)
            {
                _isRunning = false;
            }
        }
    }

    private bool ApplyPendingScene()
    {
        if (_pendingScene is null)
        {
            return false;
        }

        Scene nextScene = _pendingScene;
        _pendingScene = null;

        _activeScene?.Exit();
        _activeScene = nextScene;

        try
        {
            nextScene.Enter(ChangeScene);
        }
        catch
        {
            nextScene.Exit();
            _activeScene = null;
            throw;
        }

        return true;
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isRunning = false;

        try
        {
            _activeScene?.Exit();
        }
        finally
        {
            _activeScene = null;
            _pendingScene = null;

            _content?.Dispose();
            _content = null;

            _renderer?.Dispose();
            _renderer = null;

            if (_window != IntPtr.Zero)
            {
                SDL_DestroyWindow(_window);
                _window = IntPtr.Zero;
            }

            if (_isInitialized)
            {
                SDL_Quit();
                _isInitialized = false;
            }

            _isDisposed = true;
            GC.SuppressFinalize(this);
        }
    }
}
