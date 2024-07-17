#if TOOLS

using System.Diagnostics.CodeAnalysis;
using Godot;

namespace GodotTextureSlicer;
// This script contains only the public apis of the UnityAtlasTextureCreator

/// <summary>
///     Provides a unified window for ease of AtlasTexture creation and modification.
/// </summary>
[Tool]
public partial class UnityAtlasTextureCreator : Control
{
    /// <summary>
    ///     Initializes the window
    /// </summary>
    /// <param name="editorPlugin"></param>
    public void Initialize(EditorPlugin editorPlugin)
    {
        _editorPlugin = editorPlugin;

        var editorInterface = EditorInterface.Singleton;
        Theme = editorInterface.GetEditorTheme();
        _editorFileSystem = editorInterface.GetResourceFilesystem();
        var settings = editorInterface.GetEditorSettings();

        InitializeSaveDiscardSection();
        InitializeAtlasTextureMiniInspector();
        InitializeTopBarSection(settings);
        InitializePrimaryViewSection();
        InitializeSlicer(settings);

        UpdateControls();
        ResetInspectingMetrics();
    }

    /// <summary>
    ///     Change the editing texture
    /// </summary>
    /// <param name="newTexture">New texture for editing, pass null for abort</param>
    public void UpdateEditingTexture(Texture2D? newTexture)
    {
        if (_inspectingTex != null)
        {
            _inspectingTex.Changed -= OnTexChanged;
            _inspectingTex = null;
            _editingAtlasTexture.Clear();
            _inspectingAtlasTextureInfo = null;
            ResetInspectingMetrics();
            HideSlicerMenu();
            AtlasTextureSlicerButton!.SetPressedNoSignal(false);
        }

        _inspectingTex = newTexture;

        UpdateControls();

        if (_inspectingTex == null)
        {
            HideSlicerMenu();
            return;
        }

        _inspectingTexName = GDPath.GetFileNameWithoutExtension(_inspectingTex.ResourcePath);
        _currentSourceTexturePath = GDPath.GetDirectoryName(_inspectingTex.ResourcePath);
        RegResourceChanged(_inspectingTex, OnTexChanged);
        UpdateInspectingTexture();

        EditDrawer!.QueueRedraw();
        _requestCenter = true;
    }
}
#endif
