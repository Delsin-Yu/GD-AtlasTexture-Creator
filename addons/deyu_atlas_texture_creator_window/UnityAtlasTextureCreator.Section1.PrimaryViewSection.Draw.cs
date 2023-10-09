#if TOOLS


using System;
using System.Runtime.InteropServices;
using Godot;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;
// This script contains the api used by the Draw submodule from the Primary View Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    /// <summary>
    ///     Core method for drawing everything inside the main rect editor viewport
    /// </summary>
    private void DrawRegion()
    {
        if (m_InspectingTex is null) return;

        var base_tex = m_InspectingTex;

        var transform2D =
            new Transform2D
            {
                [2] = -m_DrawOffsets * m_DrawZoom,
                X = new(m_DrawZoom, 0),
                Y = new(0, m_DrawZoom)
            };

        var rid = EditDrawer.GetCanvasItem();
        RenderingServer.CanvasItemAddSetTransform(rid, transform2D);

        EditDrawer.DrawRect(new(Vector2.Zero, m_PreviewTex.GetSize()), new(0.5f, 0.5f, 0.5f, 0.5f), false);
        EditDrawer.DrawTexture(m_PreviewTex, Vector2.Zero);

        var color = Theme.GetColor("mono_color", "Editor");
        var selectedColor = Colors.Green;

        var selectHandle = Theme.GetIcon("EditorHandle", "EditorIcons");

        var scroll_rect = new Rect2(Vector2.Zero, base_tex.GetSize());

        if (m_IsDragging) DrawRectFrame(m_ModifyingRegionBuffer, selectHandle, selectedColor, m_DraggingHandle);
        else
        {
            foreach (var editingAtlasTextureInfo in CollectionsMarshal.AsSpan(m_EditingAtlasTexture))
            {
                if (editingAtlasTextureInfo == m_InspectingAtlasTextureInfo) continue;
                DrawRectFrame(
                    editingAtlasTextureInfo.Region,
                    selectHandle,
                    editingAtlasTextureInfo.IsTemp ? Colors.Cyan : color,
                    Dragging.None
                );
            }

            if (m_InspectingAtlasTextureInfo is not null)
                DrawRectFrame(
                    m_InspectingAtlasTextureInfo.Region,
                    selectHandle,
                    selectedColor,
                    Dragging.None
                );
        }

        if (AtlasTextureSlicerButton.ButtonPressed)
            foreach (var slicePreviewRect in m_SlicePreview)
            {
                DrawRectFrame(slicePreviewRect, selectHandle, Colors.Red, Dragging.Area);
            }

        RenderingServer.CanvasItemAddSetTransform(rid, new());

        var scroll_margin = EditDrawer.Size / m_DrawZoom;
        scroll_rect.Position -= scroll_margin;
        scroll_rect.Size += scroll_margin * 2;

        m_UpdatingScroll = true;

        HScroll.MinValue = scroll_rect.Position.X;
        HScroll.MaxValue = scroll_rect.Position.X + scroll_rect.Size.X;
        if (Mathf.Abs(scroll_rect.Position.X - (scroll_rect.Position.X + scroll_rect.Size.X)) <= scroll_margin.X) HScroll.Hide();
        else
        {
            HScroll.Show();
            HScroll.Page = scroll_margin.X;
            HScroll.Value = m_DrawOffsets.X;
        }

        VScroll.MinValue = scroll_rect.Position.Y;
        VScroll.MaxValue = scroll_rect.Position.Y + scroll_rect.Size.Y;
        if (Mathf.Abs(scroll_rect.Position.Y - (scroll_rect.Position.Y + scroll_rect.Size.Y)) <= scroll_margin.Y)
        {
            VScroll.Hide();
            m_DrawOffsets.Y = scroll_rect.Position.Y;
        }
        else
        {
            VScroll.Show();
            VScroll.Page = scroll_margin.Y;
            VScroll.Value = m_DrawOffsets.Y;
        }

        var hScrollMinSize = HScroll.GetCombinedMinimumSize();
        var vScrollMinSize = VScroll.GetCombinedMinimumSize();

        // Avoid scrollbar overlapping.
        HScroll.SetAnchorAndOffset(Side.Right, (float)Anchor.End, VScroll.Visible ? -vScrollMinSize.X : 0);
        VScroll.SetAnchorAndOffset(Side.Bottom, (float)Anchor.End, HScroll.Visible ? -hScrollMinSize.Y : 0);

        m_UpdatingScroll = false;

        if (m_RequestCenter && HScroll.MinValue < 0)
        {
            HScroll.Value = (HScroll.MinValue + HScroll.MaxValue - HScroll.Page) / 2;
            VScroll.Value = (VScroll.MinValue + VScroll.MaxValue - VScroll.Page) / 2;
            // This ensures that the view is updated correctly.
            CallDeferred(MethodName.Pan, new Vector2(1, 0));
            m_RequestCenter = false;
        }
    }

    /// <summary>
    ///     Helper method to draw a frame around a rectangle with optional handles
    /// </summary>
    private void DrawRectFrame(in Rect2 rect, Texture2D selectHandle, in Color color, Dragging drawHandleType)
    {
        GetHandlePositionsForRectFrame(rect, out var handlePositions);
        EditDrawer.DrawRect(rect, Colors.Black, false, 4 / m_DrawZoom);
        EditDrawer.DrawRect(rect, color, false, 2 / m_DrawZoom);

        var selectionHandleSize = selectHandle.GetSize() * 1.5f / m_DrawZoom;
        var selectHandleHalfSize = selectionHandleSize / 2;

        switch (drawHandleType)
        {
            case Dragging.None:
                foreach (var handlePosition in handlePositions)
                {
                    DrawHandleTextureDirect(handlePosition);
                }

                break;
            case Dragging.Area:
                break;
            case Dragging.Handle_TopLeft:
            case Dragging.Handle_Top:
            case Dragging.Handle_TopRight:
            case Dragging.Handle_Right:
            case Dragging.Handle_BottomRight:
            case Dragging.Handle_Bottom:
            case Dragging.Handle_BottomLeft:
            case Dragging.Handle_Left:
                DrawHandleTextureDirect(handlePositions[(int)drawHandleType]);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(drawHandleType), drawHandleType, null);
        }

        return;

        void DrawHandleTextureDirect(Vector2 position) =>
            EditDrawer.DrawTextureRect(selectHandle, new(position - selectHandleHalfSize, selectionHandleSize), false);
    }
}

#endif
