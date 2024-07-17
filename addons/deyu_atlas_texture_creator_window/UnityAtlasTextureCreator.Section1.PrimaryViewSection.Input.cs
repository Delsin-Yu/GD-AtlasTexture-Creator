#if TOOLS


using System.Runtime.InteropServices;
using Godot;

namespace GodotTextureSlicer;
// This script contains the api used by the Input Handling submodule from the Primary View Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    /// <summary>
    ///     Core method for handling input events related to the region editor
    /// </summary>
    private void InputRegion(InputEvent pInput)
    {
        if (_inspectingTex is null) return;

        if (ViewPanner.ProcessGuiInput(pInput, new())) return;

        switch (pInput)
        {
            case InputEventMouseMotion mouseMotionEvent:
                ProcessMouseMotion(mouseMotionEvent);
                break;
            case InputEventMouseButton mouseButtonEvent:
                ProcessMouseButton(mouseButtonEvent);
                break;
            case InputEventMagnifyGesture magnify_gesture:
                ZoomOnPosition(_drawZoom * magnify_gesture.Factor, magnify_gesture.Position);
                break;
            case InputEventPanGesture pan_gesture:
                HScroll!.Value += HScroll.Page * pan_gesture.Delta.X / 8;
                VScroll!.Value += VScroll.Page * pan_gesture.Delta.Y / 8;
                break;
        }
    }

    /// <summary>
    ///     SubMethod for update information during mouse dragging
    /// </summary>
    private void OnMouseDragUpdate(in Vector2 localMousePosition, out Dragging draggingHandleIndex, out Vector2 draggingHandlePosition, out EditingAtlasTextureInfo? inspectingAtlasTextureInfo)
    {
        var drawZoom = 11.25f / _drawZoom;

        // Handle
        var editingAtlasTextureInfosSpanView = CollectionsMarshal.AsSpan(_editingAtlasTexture);
        foreach (var editingAtlasTextureInfo in editingAtlasTextureInfosSpanView)
        {
            GetHandlePositionsForRectFrame(editingAtlasTextureInfo.Region, out var handlePositions);
            for (var index = 0; index < handlePositions.Length; index++)
            {
                var handlePosition = handlePositions[index];
                if (!(localMousePosition.DistanceTo(handlePosition) <= drawZoom)) continue;

                draggingHandleIndex = (Dragging)index;
                draggingHandlePosition = handlePosition;
                inspectingAtlasTextureInfo = editingAtlasTextureInfo;
                return;
            }
        }

        // Area
        foreach (var editingAtlasTextureInfo in editingAtlasTextureInfosSpanView)
        {
            var region = editingAtlasTextureInfo.Region;
            if (!region.HasPoint(localMousePosition)) continue;

            draggingHandleIndex = Dragging.Area;
            draggingHandlePosition = localMousePosition;
            inspectingAtlasTextureInfo = editingAtlasTextureInfo;
            return;
        }

        // Create
        draggingHandleIndex = Dragging.HandleBottomRight;
        draggingHandlePosition = localMousePosition;
        inspectingAtlasTextureInfo = null;
    }

    /// <summary>
    ///     SubMethod for update information for mouse button events
    /// </summary>
    private void ProcessMouseButton(InputEventMouseButton mouseButton)
    {
        if (!mouseButton.Pressed)
        {
            if (!_isDragging) return;
            FlushRegionModifyingBuffer();
            _isDragging = false;
            EditDrawer!.QueueRedraw();
            return;
        }

        if (!mouseButton.ButtonMask.HasFlag(MouseButtonMask.Left)) return;

        if (_isDragging) return;

        var localMousePosition = (mouseButton.Position + _drawOffsets * _drawZoom) / _drawZoom;

        OnMouseDragUpdate(
            localMousePosition,
            out _draggingHandle,
            out _draggingHandlePosition,
            out _inspectingAtlasTextureInfo
        );

        _isDragging = true;

        if (_inspectingAtlasTextureInfo is null)
        {
            _draggingMousePositionOffset = Vector2.Zero;
            _draggingHandleStartRegion = new(localMousePosition, Vector2.Zero);
            _modifyingRegionBuffer = _draggingHandleStartRegion;
            ResetInspectingMetrics();
        }
        else
        {
            _draggingMousePositionOffset = localMousePosition - _draggingHandlePosition;
            _draggingHandleStartRegion = _inspectingAtlasTextureInfo.Region;
            _modifyingRegionBuffer = _draggingHandleStartRegion;
            UpdateControls();
            UpdateInspectingMetrics(_inspectingAtlasTextureInfo);
        }

        EditDrawer!.QueueRedraw();
    }

    /// <summary>
    ///     SubMethod for update information for mouse motion events
    /// </summary>
    private void ProcessMouseMotion(InputEventMouseMotion mouseMotion)
    {
        if (!mouseMotion.ButtonMask.HasFlag(MouseButtonMask.Left)) return;

        if (!_isDragging) return;

        var newMousePosition = (mouseMotion.Position + _drawOffsets * _drawZoom) / _drawZoom;
        var diff = newMousePosition + _draggingMousePositionOffset - _draggingHandlePosition;

        var region = _draggingHandleStartRegion;

        if (_draggingHandle is Dragging.Area) region.Position += diff;
        else region = CalculateOffset(region, _draggingHandle, diff);

        if (_currentSnapMode is SnapMode.PixelSnap) region = new(region.Position.Round(), region.Size.Round());

        _modifyingRegionBuffer = region;

        EditDrawer!.QueueRedraw();
    }

    /// <summary>
    ///     Core method for calculating the new region based on the dragging type and offset
    /// </summary>
    private static Rect2 CalculateOffset(Rect2 region, Dragging dragging, Vector2 diff) =>
        dragging switch
        {
            Dragging.HandleTopLeft => region.GrowIndividual(-diff.X, -diff.Y, 0, 0),
            Dragging.HandleTop => region.GrowIndividual(0, -diff.Y, 0, 0),
            Dragging.HandleTopRight => region.GrowIndividual(0, -diff.Y, diff.X, 0),
            Dragging.HandleRight => region.GrowIndividual(0, 0, diff.X, 0),
            Dragging.HandleBottomRight => region.GrowIndividual(0, 0, diff.X, diff.Y),
            Dragging.HandleBottom => region.GrowIndividual(0, 0, 0, diff.Y),
            Dragging.HandleBottomLeft => region.GrowIndividual(-diff.X, 0, 0, diff.Y),
            Dragging.HandleLeft => region.GrowIndividual(-diff.X, 0, 0, 0),
            _ => region
        };

    /// <summary>
    ///     Pan the view
    /// </summary>
    private void Pan(Vector2 pScrollVec)
    {
        pScrollVec /= _drawZoom;
        HScroll!.Value -= pScrollVec.X;
        VScroll!.Value -= pScrollVec.Y;
    }

    /// <summary>
    ///     Creates the AtlasTexture Slice base on the given info
    /// </summary>
    private void CreateSlice(in Rect2 region, in Rect2 margin, bool filterClip)
    {
        _inspectingAtlasTextureInfo =
            EditingAtlasTextureInfo.CreateEmpty(
                region,
                _inspectingTexName!,
                margin,
                filterClip,
                _editingAtlasTexture
            );
        _editingAtlasTexture.Add(_inspectingAtlasTextureInfo);
        UpdateInspectingMetrics(_inspectingAtlasTextureInfo);
    }

    /// <summary>
    ///     Called when releasing mouse drag, this function applies the info of current dragging rect (
    ///     <see cref="_modifyingRegionBuffer" />>) into the <see cref="_inspectingAtlasTextureInfo" />
    /// </summary>
    private void FlushRegionModifyingBuffer()
    {
        if (_inspectingAtlasTextureInfo is null)
        {
            if (!_modifyingRegionBuffer.HasArea()) return;
            CreateSlice(_modifyingRegionBuffer, new(), false);
        }
        else
        {
            if (!_inspectingAtlasTextureInfo.TrySetRegion(_modifyingRegionBuffer)) return;
            UpdateInspectingMetrics(_inspectingAtlasTextureInfo);
        }

        UpdateControls();
    }
}

#endif
