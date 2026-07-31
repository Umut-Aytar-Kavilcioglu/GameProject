using static Framework.SDL3.SDL3;

namespace Framework.Engine;

internal sealed unsafe class Renderer2D : IDisposable
{
    private readonly int _logicalWidth;
    private readonly int _logicalHeight;

    private IntPtr _renderer;
    private IntPtr _sceneTarget;

    private bool _isDisposed;

    internal Renderer2D(
        IntPtr window,
        int logicalWidth,
        int logicalHeight,
        bool vsync)
    {
        if (window == IntPtr.Zero)
        {
            throw new ArgumentException(
                "Window handle cannot be zero.",
                nameof(window));
        }

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(logicalWidth);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(logicalHeight);

        _logicalWidth = logicalWidth;
        _logicalHeight = logicalHeight;

        _renderer = SDL_CreateGPURenderer(
            IntPtr.Zero,
            window);

        if (_renderer == IntPtr.Zero)
        {
            throw new InvalidOperationException(
                $"Failed to create SDL GPU renderer: {SDL_GetError()}");
        }

        try
        {
            EnsureSuccess(
                SDL_SetRenderVSync(_renderer, vsync ? 1 : 0),
                "Failed to configure VSync");

            EnsureSuccess(
                SDL_SetDefaultTextureScaleMode(
                    _renderer,
                    SDL_ScaleMode.SDL_SCALEMODE_NEAREST),
                "Failed to configure the default texture scale mode");

            SDL_Texture* target = SDL_CreateTexture(
                _renderer,
                SDL_PixelFormat.SDL_PIXELFORMAT_RGBA8888,
                SDL_TextureAccess.SDL_TEXTUREACCESS_TARGET,
                _logicalWidth,
                _logicalHeight);

            if (target == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create {_logicalWidth}x{_logicalHeight} " +
                    $"scene target: {SDL_GetError()}");
            }

            _sceneTarget = (IntPtr)target;

            EnsureSuccess(
                SDL_SetTextureScaleMode(
                    _sceneTarget,
                    SDL_ScaleMode.SDL_SCALEMODE_PIXELART),
                "Failed to configure scene target scaling");

            // Configure the window render target.
            EnsureSuccess(
                SDL_SetRenderTarget(_renderer, IntPtr.Zero),
                "Failed to select the window render target");

            EnsureSuccess(
                SDL_SetRenderLogicalPresentation(
                    _renderer,
                    _logicalWidth,
                    _logicalHeight,
                    SDL_RendererLogicalPresentation
                        .SDL_LOGICAL_PRESENTATION_LETTERBOX),
                "Failed to configure logical presentation");

            // Configure the low-resolution scene render target.
            EnsureSuccess(
                SDL_SetRenderTarget(_renderer, _sceneTarget),
                "Failed to select the scene render target");

            EnsureSuccess(
                SDL_SetRenderLogicalPresentation(
                    _renderer,
                    _logicalWidth,
                    _logicalHeight,
                    SDL_RendererLogicalPresentation
                        .SDL_LOGICAL_PRESENTATION_DISABLED),
                "Failed to disable logical presentation on the scene target");

            EnsureSuccess(
                SDL_SetRenderTarget(_renderer, IntPtr.Zero),
                "Failed to restore the window render target");
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    internal IntPtr Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_isDisposed, this);
            return _renderer;
        }
    }

    internal void Render(Scene scene)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentNullException.ThrowIfNull(scene);

        RenderSceneToTarget(scene);
        PresentSceneTarget();
    }

    private void RenderSceneToTarget(Scene scene)
    {
        EnsureSuccess(
            SDL_SetRenderTarget(_renderer, _sceneTarget),
            "Failed to select the scene render target");

        EnsureSuccess(
            SDL_SetRenderDrawColor(
                _renderer,
                30,
                30,
                30,
                255),
            "Failed to set the scene clear color");

        EnsureSuccess(
            SDL_RenderClear(_renderer),
            "Failed to clear the scene render target");

        float cameraX = MathF.Round(
            scene.Camera.Position.X,
            MidpointRounding.AwayFromZero);

        float cameraY = MathF.Round(
            scene.Camera.Position.Y,
            MidpointRounding.AwayFromZero);

        foreach (GameObject gameObject in scene.GameObjects)
        {
            if (!gameObject.IsActive) continue;

            Transform? transform =
                gameObject.GetComponent<Transform>();

            Sprite2D? sprite =
                gameObject.GetComponent<Sprite2D>();

            if (transform is null ||
                sprite is null ||
                !transform.IsActive ||
                !sprite.IsActive)
            {
                continue;
            }

            RenderSprite(sprite, transform, cameraX, cameraY);
        }
    }

    private void RenderSprite(
        Sprite2D sprite,
        Transform transform,
        float cameraX,
        float cameraY)
    {
        Texture2D texture = sprite.Texture;

        SDL_FRect source = new()
        {
            x = 0,
            y = 0,
            w = texture.Width,
            h = texture.Height
        };

        SDL_FRect destination = new()
        {
            x = MathF.Round(
                transform.Position.X - cameraX,
                MidpointRounding.AwayFromZero),

            y = MathF.Round(
                transform.Position.Y - cameraY,
                MidpointRounding.AwayFromZero),

            w = texture.Width,
            h = texture.Height
        };

        EnsureSuccess(
            SDL_RenderTexture(
                _renderer,
                texture.Handle,
                ref source,
                ref destination),
            "Failed to render sprite");
    }

    private void PresentSceneTarget()
    {
        EnsureSuccess(
            SDL_SetRenderTarget(_renderer, IntPtr.Zero),
            "Failed to select the window render target");

        EnsureSuccess(
            SDL_SetRenderDrawColor(
                _renderer,
                0,
                0,
                0,
                255),
            "Failed to set the window clear color");

        EnsureSuccess(
            SDL_RenderClear(_renderer),
            "Failed to clear the window render target");

        SDL_FRect source = new()
        {
            x = 0,
            y = 0,
            w = _logicalWidth,
            h = _logicalHeight
        };

        SDL_FRect destination = new()
        {
            x = 0,
            y = 0,
            w = _logicalWidth,
            h = _logicalHeight
        };

        EnsureSuccess(
            SDL_RenderTexture(
                _renderer,
                _sceneTarget,
                ref source,
                ref destination),
            "Failed to present the scene target");

        EnsureSuccess(
            SDL_RenderPresent(_renderer),
            "Failed to present the renderer");
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        if (_renderer != IntPtr.Zero)
        {
            // Ignore failure during cleanup.
            SDL_SetRenderTarget(_renderer, IntPtr.Zero);
        }

        if (_sceneTarget != IntPtr.Zero)
        {
            SDL_DestroyTexture(_sceneTarget);
            _sceneTarget = IntPtr.Zero;
        }

        if (_renderer != IntPtr.Zero)
        {
            SDL_DestroyRenderer(_renderer);
            _renderer = IntPtr.Zero;
        }

        _isDisposed = true;
        GC.SuppressFinalize(this);
    }

    private static void EnsureSuccess(
        SDLBool success,
        string operation)
    {
        if (!success)
        {
            throw new InvalidOperationException(
                $"{operation}: {SDL_GetError()}");
        }
    }
}
