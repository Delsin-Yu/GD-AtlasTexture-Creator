using Godot;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;
// This script contains the C# implementation of IsPixelOpaqueImpl for ImageTexture, PortableCompressedTexture2D, and CompressedTexture2D which the original methods are only available in Native Side

public partial class UnityAtlasTextureCreator
{
    private static Texture2D s_CurrentHandlingImage;
    private static Bitmap s_ImageAlphaCache;
    private static Rid s_ImageTextureRid;

    /// <summary>
    ///     Extract the Image info from the given <paramref name="texture2D" />, Cached.
    /// </summary>
    private static Image GetImageFromTexture2D(Texture2D texture2D)
    {
        if (!s_ImageTextureRid.IsValid) s_ImageTextureRid = RenderingServer.Texture2DCreate(texture2D.GetImage());

        return RenderingServer.Texture2DGet(s_ImageTextureRid);
    }

    /// <summary>
    ///     The C# implementation of IsPixelOpaqueImpl for ImageTexture, PortableCompressedTexture2D, and CompressedTexture2D
    ///     which the original methods are only available in Native Side
    /// </summary>
    private static bool IsPixelOpaqueImpl(Texture2D texture2D, int x, int y)
    {
        if (s_CurrentHandlingImage != texture2D)
        {
            s_ImageAlphaCache = null;
            s_ImageTextureRid = new();
            s_CurrentHandlingImage = texture2D;
        }

        switch (texture2D)
        {
            case ImageTexture:
            case PortableCompressedTexture2D:
            case CompressedTexture2D:
                if (s_ImageAlphaCache is null)
                {
                    var img = GetImageFromTexture2D(texture2D);
                    if (img is not null)
                    {
                        if (img.IsCompressed())
                        {
                            //must decompress, if compressed
                            var decompressed = (Image)img.Duplicate();
                            decompressed.Decompress();
                            img = decompressed;
                        }

                        s_ImageAlphaCache = new();
                        s_ImageAlphaCache.CreateFromImageAlpha(img);
                    }
                }

                if (s_ImageAlphaCache is null) return true;
                var (aw, ah) = s_ImageAlphaCache.GetSize();
                if (aw == 0 || ah == 0) return true;

                var imageSize = texture2D.GetSize();

                var x1 = (int)(x * aw / imageSize.X);
                var y1 = (int)(y * ah / imageSize.Y);

                x1 = Mathf.Clamp(x1, 0, aw);
                y1 = Mathf.Clamp(y1, 0, ah);

                return s_ImageAlphaCache.GetBit(x1, y1);
        }

        return true;
    }
}
