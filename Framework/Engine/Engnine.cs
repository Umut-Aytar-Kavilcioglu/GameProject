using static Framework.SDL3.SDL3;

namespace Framework.Engine;

public sealed class Engine : IDisposable
{
    private readonly string _title;
    private readonly int _width;
    private readonly int _height;
    private readonly bool _resizable;

    private IntPtr _window;
    private IntPtr _renderer;

    private bool _isRunning;
    private bool _isInitialized;
    private bool _isDisposed;

    public Engine(
        string title,
        int width,
        int height,
        bool resizable = false
    )
    {
        ArgumentException.ThrowIfNullOrEmpty(title);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        _title = title;
        _width = width;
        _height = height;
        _resizable = resizable;
    }

    public void Run()
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        Initialize();
        _isRunning = true;

        try
        {
            while (_isRunning)
            {
                ProcessInput();
                Update();
                Render();
            }
        }
        finally
        {
            _isRunning = false;
        }
    }

    private void Initialize()
    {
        if (_isInitialized) return;

        if (!SDL_Init(SDL_InitFlags.SDL_INIT_VIDEO))
        {
            throw new InvalidOperationException($"SDL initialization failed: {SDL_GetError()}");
        }

        SDL_WindowFlags flags = 0;

        _window = SDL_CreateWindow(_title, _width, _height, flags);

        if (_window == IntPtr.Zero)
        {
            string error = SDL_GetError();
            SDL_Quit();
            throw new InvalidOperationException($"Failed to create SDL window: {error}");
        }

        _renderer = SDL_CreateRenderer(_window, null!);

        if (_renderer == IntPtr.Zero)
        {
            string error = SDL_GetError();
            SDL_Quit();
            throw new InvalidOperationException($"Failed to create SDL renderer: {error}");
        }

        _isInitialized = true;
        Console.WriteLine($"Video driver: {SDL_GetCurrentVideoDriver()}");
    }

    public void ProcessInput()
    {
        while (SDL_PollEvent(out SDL_Event @event))
        {
            SDL_EventType eventType = (SDL_EventType)@event.type;

            if (eventType is SDL_EventType.SDL_EVENT_QUIT or SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED)
            {
                _isRunning = false;
            }
        }
    }

    public void Update()
    {

    }

    public void Render()
    {
        SDL_SetRenderDrawColor(_renderer, 30, 30, 30, 255);
        SDL_RenderClear(_renderer);
        SDL_RenderPresent(_renderer);
    }

    public void Dispose()
    {
        if (_isDisposed) return;

        _isRunning = false;

        if (_renderer != IntPtr.Zero)
        {
            SDL_DestroyRenderer(_renderer);
            _renderer = IntPtr.Zero;
        }

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
