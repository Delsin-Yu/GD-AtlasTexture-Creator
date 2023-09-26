#if TOOLS

#region

using System.Diagnostics.CodeAnalysis;
using Godot;

#endregion

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

// This script contains only the public apis of the UnityAtlasTextureCreator

/// <summary>
/// Provides a unified window for ease of AtlasTexture creation and modification.
/// </summary>
[Tool]
public partial class UnityAtlasTextureCreator : Control
{
    /// <summary>
    /// Initializes the window
    /// </summary>
    /// <param name="editorPlugin"></param>
    public void Initialize(EditorPlugin editorPlugin)
    {
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
    /// Change the editing texture
    /// </summary>
    /// <param name="newTexture">New texture for editing, pass null for abort</param>
    public void UpdateEditingTexture([AllowNull]Texture2D newTexture)
    {
        if (m_InspectingTex is not null)
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

        if (m_InspectingTex is null)
        {
            HideSlicerMenu();
            return;
        }

        m_InspectingTexName = GDPath.GetFileNameWithoutExtension(m_InspectingTex.ResourcePath);
        m_CurrentSourceTexturePath = GDPath.GetDirectoryName(m_InspectingTex.ResourcePath);
        m_InspectingTex.Changed += OnTexChanged;

        UpdateInspectingTexture();

        EditDrawer.QueueRedraw();
        m_RequestCenter = true;
    }
}
#endif
