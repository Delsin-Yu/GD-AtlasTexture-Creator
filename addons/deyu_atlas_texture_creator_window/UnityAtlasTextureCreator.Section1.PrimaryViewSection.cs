#if TOOLS


using System;
using Godot;

namespace GodotTextureSlicer;
// This script contains the exports and api used by the Primary View Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    [Export, ExportSubgroup("Primary View Section")] private Control? EditDrawer { get; set; }

    [Export] private Button? ZoomInButton { get; set; }
    [Export] private Button? ZoomResetButton { get; set; }
    [Export] private Button? ZoomOutButton { get; set; }
    [Export] private VScrollBar? VScroll { get; set; }
    [Export] private HScrollBar? HScroll { get; set; }

    private enum Dragging
    {
        None = -1,
        Area = -2,
        HandleTopLeft = 0,
        HandleTop = 1,
        HandleTopRight = 2,
        HandleRight = 3,
        HandleBottomRight = 4,
        HandleBottom = 5,
        HandleBottomLeft = 6,
        HandleLeft = 7,
    }

    /// <summary>
    ///     Initialize the primary view section with editor settings
    /// </summary>
    private void InitializePrimaryViewSection()
    {
        _previewTex = new();

        EditDrawer!.Draw += DrawRegion;
        EditDrawer.GuiInput += InputRegion;
        EditDrawer.FocusExited += ReleasePanKey;

        _drawZoom = 1.0f;

        BindZoomButtons(
            ZoomInButton!,
            "Zoom Out",
            () => ZoomOnPosition(_drawZoom / 1.5f, EditDrawer.Size / 2.0f),
            "ZoomLess"
        );
        BindZoomButtons(
            ZoomResetButton!,
            "Zoom Reset",
            () => ZoomOnPosition(1.0f, EditDrawer.Size / 2.0f),
            "ZoomReset"
        );
        BindZoomButtons(
            ZoomOutButton!,
            "Zoom In",
            () => ZoomOnPosition(_drawZoom * 1.5f, EditDrawer.Size / 2.0f),
            "ZoomMore"
        );

        RegRangeValueChanged(VScroll!, OnScrollChanged);
        RegRangeValueChanged(HScroll!, OnScrollChanged);

        EditDrawer.AddThemeStyleboxOverride("panel", Theme.GetStylebox("panel", "Tree"));


        _updatingScroll = false;

        return;

        void OnScrollChanged(double _)
        {
            if (_updatingScroll) return;

            _drawOffsets.X = (float)HScroll!.Value;
            _drawOffsets.Y = (float)VScroll!.Value;
            EditDrawer!.QueueRedraw();
        }

        void BindZoomButtons(Button button, string text, Action onPress, string editorIconName)
        {
            button.Flat = true;
            button.TooltipText = Tr(text);
            button.Icon = Theme.GetIcon(editorIconName, "EditorIcons");
            RegButtonPressed(button, onPress);
        }
    }

    private void ReleasePanKey() => ViewPanner.ReleasePanKey();

    /// <summary>
    ///     Calculate eight handle positions for a given rectangle frame
    /// </summary>
    /// <param name="rect"></param>
    /// <param name="eightHandlePositions"></param>
    private void GetHandlePositionsForRectFrame
        (
            in Rect2 rect,
            out ReadOnlySpan<Vector2> eightHandlePositions
        )
    {
        var rawEndpoints0 = rect.Position;
        var rawEndpoints1 = rect.Position + new Vector2(rect.Size.X, 0);
        var rawEndpoints2 = rect.Position + rect.Size;
        var rawEndpoints3 = rect.Position + new Vector2(0, rect.Size.Y);

        CalculateHandlePosition(rawEndpoints0, rawEndpoints3, rawEndpoints1, out var rawEndpoints0_handle1, out var rawEndpoints0_handle2);
        CalculateHandlePosition(rawEndpoints1, rawEndpoints0, rawEndpoints2, out var rawEndpoints1_handle1, out var rawEndpoints1_handle2);
        CalculateHandlePosition(rawEndpoints2, rawEndpoints1, rawEndpoints3, out var rawEndpoints2_handle1, out var rawEndpoints2_handle2);
        CalculateHandlePosition(rawEndpoints3, rawEndpoints2, rawEndpoints0, out var rawEndpoints3_handle1, out var rawEndpoints3_handle2);

        _handlePositionBuffer[0] = rawEndpoints0_handle1;
        _handlePositionBuffer[1] = rawEndpoints0_handle2;
        _handlePositionBuffer[2] = rawEndpoints1_handle1;
        _handlePositionBuffer[3] = rawEndpoints1_handle2;
        _handlePositionBuffer[4] = rawEndpoints2_handle1;
        _handlePositionBuffer[5] = rawEndpoints2_handle2;
        _handlePositionBuffer[6] = rawEndpoints3_handle1;
        _handlePositionBuffer[7] = rawEndpoints3_handle2;

        eightHandlePositions = _handlePositionBuffer.AsSpan();

        return;

        void CalculateHandlePosition
            (
                in Vector2 position,
                in Vector2 previousPosition,
                in Vector2 nextPosition,
                out Vector2 handle1,
                out Vector2 handle2
            )
        {
            var offset =
                (
                    (position - previousPosition).Normalized() +
                    (position - nextPosition).Normalized()
                )
               .Normalized() *
                10f /
                _drawZoom;

            handle1 = position + offset;

            offset = (nextPosition - position) / 2;
            offset +=
                (nextPosition - position)
               .Orthogonal()
               .Normalized() *
                10f /
                _drawZoom;

            handle2 = position + offset;
        }
    }

    /// <summary>
    ///     Zoom the view at a specific position (for view panner scroll)
    /// </summary>
    private void ZoomOnPositionScroll(float pZoom, Vector2 pPosition) => ZoomOnPosition(_drawZoom * pZoom, pPosition);

    /// <summary>
    ///     Zoom the view at a specific position
    /// </summary>
    private void ZoomOnPosition(float pZoom, Vector2 pPosition)
    {
        if (pZoom < 0.25 || pZoom > 8) return;

        var prev_zoom = _drawZoom;
        _drawZoom = pZoom;
        var ofs = pPosition;
        ofs = ofs / prev_zoom - ofs / _drawZoom;
        _drawOffsets = (_drawOffsets + ofs).Round();

        EditDrawer!.QueueRedraw();
    }
}

#endif
