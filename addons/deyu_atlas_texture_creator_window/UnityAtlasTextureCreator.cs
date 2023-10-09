#if TOOLS

using System.Diagnostics.CodeAnalysis;
using Godot;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;
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
        m_EditorPlugin = editorPlugin;

        var editorInterface = editorPlugin.GetEditorInterface();
        m_EditorFileSystem = editorInterface.GetResourceFilesystem();
        var settings = editorInterface.GetEditorSettings();
        Theme = editorInterface.GetBaseControl().Theme;

        InitializeSaveDiscardSection();
        InitializeAtlasTextureMiniInspector();
        InitializeTopBarSection(settings);
        InitializePrimaryViewSection(settings);
        InitializeSlicer(settings);

        UpdateControls();
        ResetInspectingMetrics();
    }

    /// <summary>
    ///     Change the editing texture
    /// </summary>
    /// <param name="newTexture">New texture for editing, pass null for abort</param>
    public void UpdateEditingTexture([AllowNull] Texture2D newTexture)
    {
        if (m_InspectingTex != null)
        {
            m_InspectingTex.Changed -= OnTexChanged;
            m_InspectingTex = null;
            m_EditingAtlasTexture.Clear();
            m_InspectingAtlasTextureInfo = null;
            ResetInspectingMetrics();
            HideSlicerMenu();
            AtlasTextureSlicerButton.SetPressedNoSignal(false);
        }

        m_InspectingTex = newTexture;

        UpdateControls();

        if (m_InspectingTex == null)
        {
            HideSlicerMenu();
            return;
        }

        m_InspectingTexName = GdPath.GetFileNameWithoutExtension(m_InspectingTex.ResourcePath);
        m_CurrentSourceTexturePath = GdPath.GetDirectoryName(m_InspectingTex.ResourcePath);
        RegResourceChanged(m_InspectingTex, OnTexChanged);
        UpdateInspectingTexture();

        EditDrawer.QueueRedraw();
        m_RequestCenter = true;
    }
}
#endif
