#if TOOLS


using System.Runtime.InteropServices;
using Godot;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;
// This script contains the api used by the Input Handling submodule from the Primary View Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    /// <summary>
    ///     Core method for handling input events related to the region editor
    /// </summary>
    private void InputRegion(InputEvent p_input)
    {
        if (m_InspectingTex is null) return;

        if (ViewPanner.ProcessGuiInput(p_input, new())) return;

        switch (p_input)
        {
            case InputEventMouseMotion mouseMotionEvent:
                ProcessMouseMotion(mouseMotionEvent);
                break;
            case InputEventMouseButton mouseButtonEvent:
                ProcessMouseButton(mouseButtonEvent);
                break;
            case InputEventMagnifyGesture magnify_gesture:
                ZoomOnPosition(m_DrawZoom * magnify_gesture.Factor, magnify_gesture.Position);
                break;
            case InputEventPanGesture pan_gesture:
                HScroll.Value += HScroll.Page * pan_gesture.Delta.X / 8;
                VScroll.Value += VScroll.Page * pan_gesture.Delta.Y / 8;
                break;
        }
    }

    /// <summary>
    ///     SubMethod for update information during mouse dragging
    /// </summary>
    private void OnMouseDragUpdate(in Vector2 localMousePosition, out Dragging draggingHandleIndex, out Vector2 draggingHandlePosition, out EditingAtlasTextureInfo inspectingAtlasTextureInfo)
    {
        var drawZoom = 11.25f / m_DrawZoom;

        // Handle
        var editingAtlasTextureInfosSpanView = CollectionsMarshal.AsSpan(m_EditingAtlasTexture);
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
        draggingHandleIndex = Dragging.Handle_BottomRight;
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
            if (!m_IsDragging) return;
            FlushRegionModifyingBuffer();
            m_IsDragging = false;
            EditDrawer.QueueRedraw();
            return;
        }

        if (!mouseButton.ButtonMask.HasFlag(MouseButtonMask.Left)) return;

        if (m_IsDragging) return;

        var localMousePosition = (mouseButton.Position + m_DrawOffsets * m_DrawZoom) / m_DrawZoom;

        OnMouseDragUpdate(
            localMousePosition,
            out m_DraggingHandle,
            out m_DraggingHandlePosition,
            out m_InspectingAtlasTextureInfo
        );

        m_IsDragging = true;

        if (m_InspectingAtlasTextureInfo is null)
        {
            m_DraggingMousePositionOffset = Vector2.Zero;
            m_DraggingHandleStartRegion = new(localMousePosition, Vector2.Zero);
            m_ModifyingRegionBuffer = m_DraggingHandleStartRegion;
            ResetInspectingMetrics();
        }
        else
        {
            m_DraggingMousePositionOffset = localMousePosition - m_DraggingHandlePosition;
            m_DraggingHandleStartRegion = m_InspectingAtlasTextureInfo.Region;
            m_ModifyingRegionBuffer = m_DraggingHandleStartRegion;
            UpdateControls();
            UpdateInspectingMetrics(m_InspectingAtlasTextureInfo);
        }

        EditDrawer.QueueRedraw();
    }

    /// <summary>
    ///     SubMethod for update information for mouse motion events
    /// </summary>
    private void ProcessMouseMotion(InputEventMouseMotion mouseMotion)
    {
        if (!mouseMotion.ButtonMask.HasFlag(MouseButtonMask.Left)) return;

        if (!m_IsDragging) return;

        var newMousePosition = (mouseMotion.Position + m_DrawOffsets * m_DrawZoom) / m_DrawZoom;
        var diff = newMousePosition + m_DraggingMousePositionOffset - m_DraggingHandlePosition;

        var region = m_DraggingHandleStartRegion;

        if (m_DraggingHandle is Dragging.Area) region.Position += diff;
        else region = CalculateOffset(region, m_DraggingHandle, diff);

        if (m_CurrentSnapMode is SnapMode.PixelSnap) region = new(region.Position.Round(), region.Size.Round());

        m_ModifyingRegionBuffer = region;

        EditDrawer.QueueRedraw();
    }

    /// <summary>
    ///     Core method for calculating the new region based on the dragging type and offset
    /// </summary>
    private static Rect2 CalculateOffset(Rect2 region, Dragging dragging, Vector2 diff) =>
        dragging switch
        {
            Dragging.Handle_TopLeft => region.GrowIndividual(-diff.X, -diff.Y, 0, 0),
            Dragging.Handle_Top => region.GrowIndividual(0, -diff.Y, 0, 0),
            Dragging.Handle_TopRight => region.GrowIndividual(0, -diff.Y, diff.X, 0),
            Dragging.Handle_Right => region.GrowIndividual(0, 0, diff.X, 0),
            Dragging.Handle_BottomRight => region.GrowIndividual(0, 0, diff.X, diff.Y),
            Dragging.Handle_Bottom => region.GrowIndividual(0, 0, 0, diff.Y),
            Dragging.Handle_BottomLeft => region.GrowIndividual(-diff.X, 0, 0, diff.Y),
            Dragging.Handle_Left => region.GrowIndividual(-diff.X, 0, 0, 0),
            _ => region
        };

    /// <summary>
    ///     Pan the view
    /// </summary>
    private void Pan(Vector2 p_scroll_vec)
    {
        p_scroll_vec /= m_DrawZoom;
        HScroll.Value -= p_scroll_vec.X;
        VScroll.Value -= p_scroll_vec.Y;
    }

    /// <summary>
    ///     Creates the AtlasTexture Slice base on the given info
    /// </summary>
    private void CreateSlice(in Rect2 region, in Rect2 margin, bool filterClip)
    {
        m_InspectingAtlasTextureInfo =
            EditingAtlasTextureInfo.CreateEmpty(
                region,
                m_InspectingTexName,
                margin,
                filterClip,
                m_EditingAtlasTexture
            );
        m_EditingAtlasTexture.Add(m_InspectingAtlasTextureInfo);
        UpdateInspectingMetrics(m_InspectingAtlasTextureInfo);
    }

    /// <summary>
    ///     Called when releasing mouse drag, this function applies the info of current dragging rect (
    ///     <see cref="m_ModifyingRegionBuffer" />>) into the <see cref="m_InspectingAtlasTextureInfo" />
    /// </summary>
    private void FlushRegionModifyingBuffer()
    {
        if (m_InspectingAtlasTextureInfo is null)
        {
            if (!m_ModifyingRegionBuffer.HasArea()) return;
            CreateSlice(m_ModifyingRegionBuffer, new(), false);
        }
        else
        {
            if (!m_InspectingAtlasTextureInfo.TrySetRegion(m_ModifyingRegionBuffer)) return;
            UpdateInspectingMetrics(m_InspectingAtlasTextureInfo);
        }

        UpdateControls();
    }
}

#endif
