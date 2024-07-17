#if TOOLS

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Godot;
using Godot.Collections;

namespace GodotTextureSlicer;
// This script contains the private fields and general api used by the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    private readonly List<EditingAtlasTextureInfo> _editingAtlasTexture = new();
    private readonly Vector2[] _handlePositionBuffer = new Vector2[8];
    private readonly Array _oneLengthArray = new(new Variant[1]);
    private string? _currentSourceTexturePath;
    private Dragging _draggingHandle;
    private Vector2 _draggingHandlePosition;
    private Rect2 _draggingHandleStartRegion;
    private Vector2 _draggingMousePositionOffset;
    private Vector2 _drawOffsets;
    private float _drawZoom;
    private EditorFileSystem? _editorFileSystem;
    private EditorPlugin? _editorPlugin;
    private EditorUndoRedoManager? _editorUndoRedoManager;

    private EditingAtlasTextureInfo? _inspectingAtlasTextureInfo;
    private Texture2D? _inspectingTex;
    private string? _inspectingTexName;
    private bool _isDragging;
    private Rect2 _modifyingRegionBuffer;
    private CanvasTexture? _previewTex;
    private bool _requestCenter;
    private bool _updatingScroll;

    // ReSharper disable once IdentifierTypo
    private ViewPannerCSharpImpl? _viewPanner;

    private ViewPannerCSharpImpl ViewPanner
    {
        get
        {
            if (_viewPanner is not null) return _viewPanner;
            
            var settings =  EditorInterface.Singleton.GetEditorSettings();

            _viewPanner = new(
                Pan,
                ZoomOnPositionScroll,
                settings.Get("editors/panning/sub_editors_panning_scheme").As<ViewPannerCSharpImpl.ControlScheme>(),
                new(), // settings.GetShortcut("canvas_ite_editor/pan_view"); // This api only exists in native side, Sad :(
                settings.Get("editors/panning/simple_panning").As<bool>()
            );

            return _viewPanner;
        }
    }


    /// <summary>
    ///     Update all UI controls based on the status of <see cref="_inspectingTex" />, <see cref="_editingAtlasTexture" />
    ///     and <see cref="_inspectingAtlasTextureInfo" />
    /// </summary>
    private void UpdateControls()
    {
        var isEditingAsset = _inspectingTex is not null;
        _oneLengthArray[0] = !isEditingAsset;
        PropagateCall(BaseButton.MethodName.SetDisabled, _oneLengthArray);
        _oneLengthArray[0] = isEditingAsset;
        PropagateCall(LineEdit.MethodName.SetEditable, _oneLengthArray);

        Modulate = isEditingAsset ? Colors.White : new(1, 1, 1, 0.5f);

        var hasPendingChanges = _editingAtlasTexture.Any(x => x.Modified);

        Name =
            hasPendingChanges ?
                "AtlasTexture Creator (*)" :
                "AtlasTexture Creator";

        DiscardButton!.Disabled = !hasPendingChanges;
        SaveAndUpdateButton!.Disabled = !hasPendingChanges;

        var isInspectingAtlasTexture = _inspectingAtlasTextureInfo is not null;

        _oneLengthArray[0] = !isInspectingAtlasTexture;
        MiniInspectorWindow!.PropagateCall(BaseButton.MethodName.SetDisabled, _oneLengthArray);
        _oneLengthArray[0] = isInspectingAtlasTexture;
        MiniInspectorWindow.PropagateCall(LineEdit.MethodName.SetEditable, _oneLengthArray);
        MiniInspectorWindow.Modulate = _inspectingAtlasTextureInfo is not null ? Colors.White : new(1, 1, 1, 0.5f);

        EditDrawer!.QueueRedraw();
    }

    /// <summary>
    ///     Method called when the inspected texture is changed
    /// </summary>
    private void OnTexChanged()
    {
        if (!Visible) return;

        UpdateInspectingTexture();
    }

    /// <summary>
    ///     Method to update the inspection of the current texture
    /// </summary>
    private void UpdateInspectingTexture()
    {
        var texture = _inspectingTex;

        if (texture is null)
        {
            _previewTex!.DiffuseTexture = null;
            ZoomOnPosition(1.0f, EditDrawer!.Size / 2.0f);
            HScroll!.Hide();
            VScroll!.Hide();
            EditDrawer.QueueRedraw();
            return;
        }

        _previewTex!.TextureFilter = TextureFilterEnum.NearestWithMipmaps;
        _previewTex.DiffuseTexture = texture;

        EditDrawer!.QueueRedraw();
    }
}

#endif
