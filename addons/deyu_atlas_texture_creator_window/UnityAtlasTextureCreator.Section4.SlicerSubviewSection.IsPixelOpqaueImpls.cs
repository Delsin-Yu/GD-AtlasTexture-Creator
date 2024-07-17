using Godot;

namespace GodotTextureSlicer;
// This script contains the C# implementation of IsPixelOpaqueImpl for ImageTexture, PortableCompressedTexture2D, and CompressedTexture2D which the original methods are only available in Native Side

public partial class UnityAtlasTextureCreator
{
    private static Texture2D? CurrentHandlingImage;
    private static Bitmap? ImageAlphaCache;
    private static Rid ImageTextureRid;

    /// <summary>
    ///     Extract the Image info from the given <paramref name="texture2D" />, Cached.
    /// </summary>
    private static Image GetImageFromTexture2D(Texture2D texture2D)
    {
        if (!ImageTextureRid.IsValid) ImageTextureRid = RenderingServer.Texture2DCreate(texture2D.GetImage());

        return RenderingServer.Texture2DGet(ImageTextureRid);
    }

    /// <summary>
    ///     The C# implementation of IsPixelOpaqueImpl for ImageTexture, PortableCompressedTexture2D, and CompressedTexture2D
    ///     which the original methods are only available in Native Side
    /// </summary>
    private static bool IsPixelOpaqueImpl(Texture2D texture2D, int x, int y)
    {
        if (CurrentHandlingImage != texture2D)
        {
            ImageAlphaCache = null;
            ImageTextureRid = new();
            CurrentHandlingImage = texture2D;
        }

        switch (texture2D)
        {
            case ImageTexture:
            case PortableCompressedTexture2D:
            case CompressedTexture2D:
                if (ImageAlphaCache is null)
                {
                    var img = GetImageFromTexture2D(texture2D);

                    if (img.IsCompressed())
                    {
                        //must decompress, if compressed
                        var decompressed = (Image)img.Duplicate();
                        decompressed.Decompress();
                        img = decompressed;
                    }

                    ImageAlphaCache = new();
                    ImageAlphaCache.CreateFromImageAlpha(img);
                }

                var (aw, ah) = ImageAlphaCache.GetSize();
                if (aw == 0 || ah == 0) return true;

                var imageSize = texture2D.GetSize();

                var x1 = (int)(x * aw / imageSize.X);
                var y1 = (int)(y * ah / imageSize.Y);

                x1 = Mathf.Clamp(x1, 0, aw);
                y1 = Mathf.Clamp(y1, 0, ah);

                return ImageAlphaCache.GetBit(x1, y1);
        }

        return true;
    }
}
