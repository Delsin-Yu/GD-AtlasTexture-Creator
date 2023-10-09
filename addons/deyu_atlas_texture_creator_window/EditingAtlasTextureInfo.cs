#if TOOLS

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

public class EditingAtlasTextureInfo
{
    private static readonly HashSet<string> s_InvalidFileNameChars = Path.GetInvalidFileNameChars().Select(x => x.ToString()).ToHashSet();
    private AtlasTexture m_BackingAtlasTexture;
    private string m_ResourcePath;

    private EditingAtlasTextureInfo(AtlasTexture backingAtlasTexture, Rect2 region, Rect2 margin, bool filterClip, string name, string resourcePath)
    {
        m_BackingAtlasTexture = backingAtlasTexture;
        Region = region;
        Margin = margin;
        FilterClip = filterClip;
        Name = name;
        m_ResourcePath = resourcePath;
    }

    public bool IsTemp => m_BackingAtlasTexture is null;
    public string Name { get; private set; }
    public Rect2 Region { get; private set; }
    public Rect2 Margin { get; private set; }
    public bool FilterClip { get; private set; }

    public bool Modified { get; private set; }

    private static class GdPath
    {
        public static string Combine(string pathA, string pathB)
        {
            var newPath = Path.Combine(pathA, pathB);
            return ToGdPath(newPath);
        }

        public static string ToGdPath(string path)
        {
            var unixStyledPath = path.Replace("\\", "/");
            return unixStyledPath.Insert(unixStyledPath.IndexOf('/'), "/");
        }
    }

    public static EditingAtlasTextureInfo Create((AtlasTexture atlasTexture, string resourcePath) data)
    {
        var backingAtlasTexture = data.atlasTexture;
        var dataResourcePath = data.resourcePath;

        return new(
            backingAtlasTexture,
            backingAtlasTexture.Region,
            backingAtlasTexture.Margin,
            backingAtlasTexture.FilterClip,
            Path.GetFileNameWithoutExtension(dataResourcePath),
            dataResourcePath
        );
    }

    public static EditingAtlasTextureInfo CreateEmpty(in Rect2 region, string textureName, in Rect2 margin, bool filterClip, IEnumerable<EditingAtlasTextureInfo> existingAtlasTextures)
    {
        var textureNameLower = textureName.ToLower();
        var existingAtlasTextureNamesLowerCase = existingAtlasTextures.Select(x => x.Name.ToLower()).ToHashSet();

        var nameCounter = 0;
        string nameCandidate;
        do
        {
            nameCandidate = $"{textureNameLower}_{nameCounter++}";
        }
        while (existingAtlasTextureNamesLowerCase.Contains(nameCandidate));

        var info =
            new EditingAtlasTextureInfo(null, region, margin, filterClip, null, null)
            {
                Modified = true,
                Name = nameCandidate
            };

        return info;
    }

    public bool TrySetName(in string name)
    {
        if (Name == name) return false;
        Name = name;
        Modified = true;
        return true;
    }

    public bool TrySetRegion(in Rect2 rect2)
    {
        if (Region == rect2) return false;
        Region = rect2;
        Modified = true;
        return true;
    }

    public bool TrySetMargin(in Rect2 rect2)
    {
        if (Margin == rect2) return false;
        Margin = rect2;
        Modified = true;
        return true;
    }

    public bool TrySetFilterClip(in bool enable)
    {
        if (FilterClip == enable) return false;
        FilterClip = enable;
        Modified = true;
        return true;
    }

    public string ApplyChanges(Texture2D sourceTexture, string sourceTextureDirectory)
    {
        if (!Modified) return null;

        if (m_BackingAtlasTexture == null)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                GD.PrintErr("AtlasTexture.Name is Null or WhiteSpace!");
                return null;
            }

            foreach (var invalidFileNameChar in s_InvalidFileNameChars)
            {
                Name = Name.Replace(invalidFileNameChar, string.Empty);
            }

            m_BackingAtlasTexture =
                new()
                {
                    Atlas = sourceTexture
                };
            m_ResourcePath = GdPath.Combine(sourceTextureDirectory, $"{Name}.tres");
        }

        m_BackingAtlasTexture.Region = Region;
        m_BackingAtlasTexture.Margin = Margin;
        m_BackingAtlasTexture.FilterClip = FilterClip;
        Modified = false;
        m_BackingAtlasTexture.TakeOverPath(m_ResourcePath);
        ResourceSaver.Save(m_BackingAtlasTexture, m_ResourcePath);
        return m_ResourcePath;
    }

    public void DiscardChanges()
    {
        if (!Modified) return;

        if (m_BackingAtlasTexture == null) return;

        Region = m_BackingAtlasTexture.Region;
        Margin = m_BackingAtlasTexture.Margin;
        FilterClip = m_BackingAtlasTexture.FilterClip;
        Modified = false;
    }
}
#endif
