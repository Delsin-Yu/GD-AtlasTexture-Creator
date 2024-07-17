#if TOOLS

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotTextureSlicer;
// This script contains the exports and api used by the Top Bar Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    private SnapMode _currentSnapMode = SnapMode.NoneSnap;

    [Export, ExportSubgroup("Top Bar Section")] private OptionButton? SnapModeButton { get; set; }
    [Export] private CheckButton? AtlasTextureSlicerButton { get; set; }
    [Export] private Button? ScanAtlasInFolderButton { get; set; }
    [Export] private Button? ScanAtlasInProjectButton { get; set; }

    private SnapMode CurrentSnapMode => _currentSnapMode;

    private void SetCurrentSnapMode(SnapMode value)
    {
        _currentSnapMode = value;
        SetSpinBoxMode(CurrentSnapMode is SnapMode.PixelSnap);
        EditDrawer?.QueueRedraw();
    }

    private enum SnapMode { NoneSnap, PixelSnap }

    private enum ScanMode { SourceFolderOnly, WholeProject }

    /// <summary>
    ///     Initialize the top bar section with editor settings
    /// </summary>
    /// <param name="settings"></param>
    private void InitializeTopBarSection(EditorSettings settings)
    {
        SetCurrentSnapMode(
            settings
                .GetProjectMetadata(
                    "atlas_texture_editor",
                    "snap_mode",
                    Variant.From(SnapMode.PixelSnap)
                )
                .As<SnapMode>()
        );

        RegOptionButtonItemSelected(SnapModeButton!, pMode => SetCurrentSnapMode((SnapMode)pMode));
        RegButtonPressed(ScanAtlasInFolderButton!, () => ScanAtlasTexture(ScanMode.SourceFolderOnly, _editingAtlasTexture));
        RegButtonPressed(ScanAtlasInProjectButton!, () => ScanAtlasTexture(ScanMode.WholeProject, _editingAtlasTexture));

        SnapModeButton!.Selected = (int)CurrentSnapMode;
    }

    /// <summary>
    ///     Scan and populate atlas textures based on the selected scan mode
    /// </summary>
    private void ScanAtlasTexture(ScanMode scanMode, List<EditingAtlasTextureInfo> editingAtlasTextureInfoCache)
    {
        editingAtlasTextureInfoCache.Clear();
        _inspectingAtlasTextureInfo = null;

        var collection = new List<(AtlasTexture, string)>();
        switch (scanMode)
        {
            case ScanMode.SourceFolderOnly:
                var sourcePath = _inspectingTex!.ResourcePath;
                var dirPath = GDPath.GetDirectoryName(sourcePath);
                EditorFileSystemDirectory directory;
                directory = _editorFileSystem!.GetFilesystemPath(dirPath);
                FindMatchingSourceTextureInDirectory(_inspectingTex, collection, directory);
                break;
            case ScanMode.WholeProject:
                directory = _editorFileSystem!.GetFilesystem();
                FindMatchingSourceTextureInDirectoryRecursive(_inspectingTex!, collection, directory);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scanMode), scanMode, null);
        }

        editingAtlasTextureInfoCache.AddRange(collection.Select(EditingAtlasTextureInfo.Create));
        ResetInspectingMetrics();
        UpdateControls();
    }

    /// <summary>
    ///     Recursive method to find matching source textures in a directory and its subdirectories
    /// </summary>
    private static void FindMatchingSourceTextureInDirectoryRecursive(Texture2D sourceTexture, ICollection<(AtlasTexture, string)> matchedAtlasTexture, EditorFileSystemDirectory directory)
    {
        FindMatchingSourceTextureInDirectory(sourceTexture, matchedAtlasTexture, directory);
        var subDirCount = directory.GetSubdirCount();

        for (var i = 0; i < subDirCount; i++)
        {
            var subDir = directory.GetSubdir(i);
            FindMatchingSourceTextureInDirectoryRecursive(sourceTexture, matchedAtlasTexture, subDir);
        }
    }

    /// <summary>
    ///     Scans and acquire the <see cref="AtlasTexture" /> with the <see cref="AtlasTexture.Atlas" /> matching the provided
    ///     <paramref name="sourceTexture" /> from the providing <paramref name="directory" />
    /// </summary>
    private static void FindMatchingSourceTextureInDirectory(Texture2D sourceTexture, ICollection<(AtlasTexture, string)> matchedAtlasTexture, EditorFileSystemDirectory directory)
    {
        var fileCount = directory.GetFileCount();

        for (var i = 0; i < fileCount; i++)
        {
            var filePath = directory.GetFilePath(i);
            var resource = ResourceLoader.Load(filePath, cacheMode : ResourceLoader.CacheMode.Ignore);
            if (resource is AtlasTexture atlasTexture && atlasTexture.Atlas == sourceTexture) matchedAtlasTexture.Add((atlasTexture, filePath));
        }
    }
}

#endif
