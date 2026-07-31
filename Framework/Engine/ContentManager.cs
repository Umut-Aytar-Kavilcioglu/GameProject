using static Framework.SDL3.SDL3;

namespace Framework.Engine;

public sealed unsafe class ContentManager : IDisposable
{
    private readonly IntPtr _renderer;
    private readonly Dictionary<string, Texture2D> _textures =
        new(StringComparer.Ordinal);

    private bool _isDisposed;

    internal ContentManager(IntPtr renderer)
    {
        if (renderer == IntPtr.Zero)
        {
            throw new ArgumentException(
                "Renderer handle cannot be zero.",
                nameof(renderer));
        }

        _renderer = renderer;
    }

    public Texture2D LoadTexture(string name, string path)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        if (_textures.ContainsKey(name))
        {
            throw new InvalidOperationException(
                $"A texture named '{name}' is already loaded.");
        }

        SDL_Surface* surface = SDL_LoadPNG(path);

        if (surface == null)
        {
            throw new InvalidOperationException(
                $"Failed to load PNG '{path}': {SDL_GetError()}");
        }

        IntPtr textureHandle = IntPtr.Zero;

        try
        {
            SDL_Texture* texture =
                SDL_CreateTextureFromSurface(_renderer, (IntPtr)surface);

            if (texture == null)
            {
                throw new InvalidOperationException(
                    $"Failed to create texture '{name}': {SDL_GetError()}");
            }

            textureHandle = (IntPtr)texture;

            if (!SDL_SetTextureScaleMode(
                    textureHandle,
                    SDL_ScaleMode.SDL_SCALEMODE_NEAREST))
            {
                throw new InvalidOperationException(
                    $"Failed to configure texture '{name}': {SDL_GetError()}");
            }

            if (!SDL_GetTextureSize(
                    textureHandle,
                    out float width,
                    out float height))
            {
                throw new InvalidOperationException(
                    $"Failed to get texture size for '{name}': " +
                    SDL_GetError());
            }

            Texture2D result = new(
                textureHandle,
                checked((int)width),
                checked((int)height));

            try
            {
                _textures.Add(name, result);
            }
            catch
            {
                result.Destroy();
                throw;
            }

            textureHandle = IntPtr.Zero;
            return result;
        }
        finally
        {
            if (textureHandle != IntPtr.Zero)
            {
                SDL_DestroyTexture(textureHandle);
            }

            SDL_DestroySurface((IntPtr)surface);
        }
    }

    public Texture2D GetTexture(string name)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (_textures.TryGetValue(name, out Texture2D? texture))
        {
            return texture;
        }

        throw new KeyNotFoundException(
            $"No texture named '{name}' has been loaded.");
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        foreach (Texture2D texture in _textures.Values)
        {
            texture.Destroy();
        }

        _textures.Clear();
        _isDisposed = true;

        GC.SuppressFinalize(this);
    }
}
