﻿#if TOOLS

#region

using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

#endregion

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

// This script contains the private fields and general api used by the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    private readonly List<EditingAtlasTextureInfo> m_EditingAtlasTexture = new();
    private readonly Vector2[] m_HandlePositionBuffer = new Vector2[8];
    private readonly Array m_OneLengthArray = new(new Variant[1]);
    private string m_CurrentSourceTexturePath;
    private Vector2 m_DrawOffsets;
    private float m_DrawZoom;
    private EditorFileSystem m_EditorFileSystem;
    private EditorUndoRedoManager m_EditorUndoRedoManager;
    private Texture2D m_InspectingTex;
    private string m_InspectingTexName;
    private CanvasTexture m_PreviewTex;
    private bool m_RequestCenter;
    private bool m_UpdatingScroll;

    // ReSharper disable once IdentifierTypo
    private ViewPannerCSharpImpl m_ViewPanner;

    private EditingAtlasTextureInfo m_InspectingAtlasTextureInfo;
    private bool m_IsDragging;
    private Dragging m_DraggingHandle;
    private Rect2 m_ModifyingRegionBuffer;
    private Rect2 m_DraggingHandleStartRegion;
    private Vector2 m_DraggingHandlePosition;
    private Vector2 m_DraggingMousePositionOffset;
    

    /// <summary>
    /// Update all UI controls based on the status of <see cref="m_InspectingTex"/>, <see cref="m_EditingAtlasTexture"/> and <see cref="m_InspectingAtlasTextureInfo"/>
    /// </summary>
    private void UpdateControls()
    {
        var isEditingAsset = m_InspectingTex is not null;
        m_OneLengthArray[0] = !isEditingAsset;
        PropagateCall(BaseButton.MethodName.SetDisabled, m_OneLengthArray);
        m_OneLengthArray[0] = isEditingAsset;
        PropagateCall(LineEdit.MethodName.SetEditable, m_OneLengthArray);

        Modulate = isEditingAsset ? Colors.White : new(1, 1, 1, 0.5f);

        var hasPendingChanges = m_EditingAtlasTexture.Any(x => x.Modified);

        Name =
            hasPendingChanges ?
                "AtlasTexture Creator (*)" :
                "AtlasTexture Creator";

        DiscardButton.Disabled = !hasPendingChanges;
        SaveAndUpdateButton.Disabled = !hasPendingChanges;

        var isInspectingAtlasTexture = m_InspectingAtlasTextureInfo is not null;

        m_OneLengthArray[0] = !isInspectingAtlasTexture;
        MiniInspectorWindow.PropagateCall(BaseButton.MethodName.SetDisabled, m_OneLengthArray);
        m_OneLengthArray[0] = isInspectingAtlasTexture;
        MiniInspectorWindow.PropagateCall(LineEdit.MethodName.SetEditable, m_OneLengthArray);
        MiniInspectorWindow.Modulate = m_InspectingAtlasTextureInfo is not null ? Colors.White : new(1, 1, 1, 0.5f);

        EditDrawer.QueueRedraw();
    }

    /// <summary>
    /// Method called when the inspected texture is changed
    /// </summary>
    private void OnTexChanged()
    {
        if (!Visible) return;

        UpdateInspectingTexture();
    }

    /// <summary>
    /// Method to update the inspection of the current texture
    /// </summary>
    private void UpdateInspectingTexture()
    {
        var texture = m_InspectingTex;

        if (texture is null)
        {
            m_PreviewTex.DiffuseTexture = null;
            ZoomOnPosition(1.0f, EditDrawer.Size / 2.0f);
            HScroll.Hide();
            VScroll.Hide();
            EditDrawer.QueueRedraw();
            return;
        }

        m_PreviewTex.TextureFilter = TextureFilterEnum.NearestWithMipmaps;
        m_PreviewTex.DiffuseTexture = texture;

        EditDrawer.QueueRedraw();
    }
}

#endif
