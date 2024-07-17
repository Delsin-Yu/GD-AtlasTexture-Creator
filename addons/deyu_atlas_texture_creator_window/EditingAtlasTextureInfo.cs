#if TOOLS

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Godot;

namespace GodotTextureSlicer;

class EditingAtlasTextureInfo
{
    private static readonly HashSet<string> SInvalidFileNameChars = Path.GetInvalidFileNameChars().Select(x => x.ToString()).ToHashSet();
    private AtlasTexture? _backingAtlasTexture;
    private string? _resourcePath;

    private EditingAtlasTextureInfo(AtlasTexture? backingAtlasTexture, Rect2 region, Rect2 margin, bool filterClip, string? name, string? resourcePath)
    {
        _backingAtlasTexture = backingAtlasTexture;
        Region = region;
        Margin = margin;
        FilterClip = filterClip;
        Name = name;
        _resourcePath = resourcePath;
    }

    public bool IsTemp => _backingAtlasTexture is null;
    public string? Name { get; private set; }
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
        var existingAtlasTextureNamesLowerCase = existingAtlasTextures.Select(x => x.Name?.ToLower()).ToHashSet();

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

    public string? ApplyChanges(Texture2D sourceTexture, string sourceTextureDirectory)
    {
        if (!Modified) return null;

        if (_backingAtlasTexture == null)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                GD.PrintErr("AtlasTexture.Name is Null or WhiteSpace!");
                return null;
            }

            foreach (var invalidFileNameChar in SInvalidFileNameChars)
            {
                Name = Name.Replace(invalidFileNameChar, string.Empty);
            }

            _backingAtlasTexture =
                new()
                {
                    Atlas = sourceTexture
                };
            _resourcePath = GdPath.Combine(sourceTextureDirectory, $"{Name}.tres");
        }

        _backingAtlasTexture.Region = Region;
        _backingAtlasTexture.Margin = Margin;
        _backingAtlasTexture.FilterClip = FilterClip;
        Modified = false;
        _backingAtlasTexture.TakeOverPath(_resourcePath);
        ResourceSaver.Save(_backingAtlasTexture, _resourcePath);
        return _resourcePath;
    }

    public void DiscardChanges()
    {
        if (!Modified) return;

        if (_backingAtlasTexture == null) return;

        Region = _backingAtlasTexture.Region;
        Margin = _backingAtlasTexture.Margin;
        FilterClip = _backingAtlasTexture.FilterClip;
        Modified = false;
    }
}
#endif
