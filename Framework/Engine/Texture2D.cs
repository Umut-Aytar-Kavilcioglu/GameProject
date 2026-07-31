using static Framework.SDL3.SDL3;

namespace Framework.Engine;

public sealed class Texture2D
{
    private IntPtr _handle;

    internal Texture2D(IntPtr handle, int width, int height)
    {
        if (handle == IntPtr.Zero)
        {
            throw new ArgumentException("Texture handle cannot be zero.", nameof(handle));
        }

        _handle = handle;
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    internal IntPtr Handle
    {
        get
        {
            ObjectDisposedException.ThrowIf(_handle == IntPtr.Zero, this);
            return _handle;
        }
    }

    internal void Destroy()
    {
        if (_handle == IntPtr.Zero) return;
        SDL_DestroyTexture(_handle);
        _handle = IntPtr.Zero;
    }
}
