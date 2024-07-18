using Godot;

namespace GodotTextureSlicer;
// This script contains the C# implementation of IsPixelOpaqueImpl for ImageTexture, PortableCompressedTexture2D, and CompressedTexture2D which the original methods are only available in Native Side

public partial class UnityAtlasTextureCreator
{
    private static Texture2D? _currentHandlingImage;
    private static Bitmap? _imageAlphaCache;
    private static Rid _imageTextureRid;

    /// <summary>
    ///     Extract the Image info from the given <paramref name="texture2D" />, Cached.
    /// </summary>
    private static Image GetImageFromTexture2D(Texture2D texture2D)
    {
        if (!_imageTextureRid.IsValid) _imageTextureRid = RenderingServer.Texture2DCreate(texture2D.GetImage());

        return RenderingServer.Texture2DGet(_imageTextureRid);
    }

    /// <summary>
    ///     The C# implementation of IsPixelOpaqueImpl for ImageTexture, PortableCompressedTexture2D, and CompressedTexture2D
    ///     which the original methods are only available in Native Side
    /// </summary>
    private static bool IsPixelOpaqueImpl(Texture2D texture2D, int x, int y)
    {
        if (_currentHandlingImage != texture2D)
        {
            _imageAlphaCache = null;
            _imageTextureRid = new();
            _currentHandlingImage = texture2D;
        }

        switch (texture2D)
        {
            case ImageTexture:
            case PortableCompressedTexture2D:
            case CompressedTexture2D:
                if (_imageAlphaCache is null)
                {
                    var img = GetImageFromTexture2D(texture2D);

                    if (img.IsCompressed())
                    {
                        //must decompress, if compressed
                        var decompressed = (Image)img.Duplicate();
                        decompressed.Decompress();
                        img = decompressed;
                    }

                    _imageAlphaCache = new();
                    _imageAlphaCache.CreateFromImageAlpha(img);
                }

                var (aw, ah) = _imageAlphaCache.GetSize();
                if (aw == 0 || ah == 0) return true;

                var imageSize = texture2D.GetSize();

                var x1 = (int)(x * aw / imageSize.X);
                var y1 = (int)(y * ah / imageSize.Y);

                x1 = Mathf.Clamp(x1, 0, aw);
                y1 = Mathf.Clamp(y1, 0, ah);

                return _imageAlphaCache.GetBit(x1, y1);
        }

        return true;
    }
}
