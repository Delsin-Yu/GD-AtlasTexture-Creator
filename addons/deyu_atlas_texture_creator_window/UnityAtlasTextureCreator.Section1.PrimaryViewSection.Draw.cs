#if TOOLS


using System;
using System.Runtime.InteropServices;
using Godot;

namespace GodotTextureSlicer;
// This script contains the api used by the Draw submodule from the Primary View Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    /// <summary>
    ///     Core method for drawing everything inside the main rect editor viewport
    /// </summary>
    private void DrawRegion()
    {
        if (_inspectingTex is null) return;

        var baseTex = _inspectingTex;

        var transform2D =
            new Transform2D
            {
                [2] = -_drawOffsets * _drawZoom,
                X = new(_drawZoom, 0),
                Y = new(0, _drawZoom)
            };

        var rid = EditDrawer!.GetCanvasItem();
        RenderingServer.CanvasItemAddSetTransform(rid, transform2D);

        EditDrawer.DrawRect(new(Vector2.Zero, _previewTex!.GetSize()), new(0.5f, 0.5f, 0.5f, 0.5f), false);
        EditDrawer.DrawTexture(_previewTex, Vector2.Zero);

        var color = Theme.GetColor("mono_color", "Editor");
        var selectedColor = Colors.Green;

        var selectHandle = Theme.GetIcon("EditorHandle", "EditorIcons");

        var scrollRect = new Rect2(Vector2.Zero, baseTex.GetSize());

        if (_isDragging) DrawRectFrame(_modifyingRegionBuffer, selectHandle, selectedColor, _draggingHandle);
        else
        {
            foreach (var editingAtlasTextureInfo in CollectionsMarshal.AsSpan(_editingAtlasTexture))
            {
                if (editingAtlasTextureInfo == _inspectingAtlasTextureInfo) continue;
                DrawRectFrame(
                    editingAtlasTextureInfo.Region,
                    selectHandle,
                    editingAtlasTextureInfo.IsTemp ? Colors.Cyan : color,
                    Dragging.Area
                );
            }

            if (_inspectingAtlasTextureInfo is not null)
                DrawRectFrame(
                    _inspectingAtlasTextureInfo.Region,
                    selectHandle,
                    selectedColor,
                    Dragging.None
                );
        }

        if (AtlasTextureSlicerButton!.ButtonPressed)
            foreach (var slicePreviewRect in _slicePreview)
            {
                DrawRectFrame(slicePreviewRect, selectHandle, Colors.Red, Dragging.Area);
            }

        RenderingServer.CanvasItemAddSetTransform(rid, new());

        var scrollMargin = EditDrawer.Size / _drawZoom;
        scrollRect.Position -= scrollMargin;
        scrollRect.Size += scrollMargin * 2;

        _updatingScroll = true;

        HScroll!.MinValue = scrollRect.Position.X;
        HScroll.MaxValue = scrollRect.Position.X + scrollRect.Size.X;
        if (Mathf.Abs(scrollRect.Position.X - (scrollRect.Position.X + scrollRect.Size.X)) <= scrollMargin.X) HScroll.Hide();
        else
        {
            HScroll.Show();
            HScroll.Page = scrollMargin.X;
            HScroll.Value = _drawOffsets.X;
        }

        VScroll!.MinValue = scrollRect.Position.Y;
        VScroll.MaxValue = scrollRect.Position.Y + scrollRect.Size.Y;
        if (Mathf.Abs(scrollRect.Position.Y - (scrollRect.Position.Y + scrollRect.Size.Y)) <= scrollMargin.Y)
        {
            VScroll.Hide();
            _drawOffsets.Y = scrollRect.Position.Y;
        }
        else
        {
            VScroll.Show();
            VScroll.Page = scrollMargin.Y;
            VScroll.Value = _drawOffsets.Y;
        }

        var hScrollMinSize = HScroll.GetCombinedMinimumSize();
        var vScrollMinSize = VScroll.GetCombinedMinimumSize();

        // Avoid scrollbar overlapping.
        HScroll.SetAnchorAndOffset(Side.Right, (float)Anchor.End, VScroll.Visible ? -vScrollMinSize.X : 0);
        VScroll.SetAnchorAndOffset(Side.Bottom, (float)Anchor.End, HScroll.Visible ? -hScrollMinSize.Y : 0);

        _updatingScroll = false;

        if (!_requestCenter || !(HScroll.MinValue < 0)) return;
        
        HScroll.Value = (HScroll.MinValue + HScroll.MaxValue - HScroll.Page) / 2;
        VScroll.Value = (VScroll.MinValue + VScroll.MaxValue - VScroll.Page) / 2;
        // This ensures that the view is updated correctly.
        CallDeferred(MethodName.Pan, new Vector2(1, 0));
        _requestCenter = false;
    }

    /// <summary>
    ///     Helper method to draw a frame around a rectangle with optional handles
    /// </summary>
    private void DrawRectFrame(in Rect2 rect, Texture2D selectHandle, in Color color, Dragging drawHandleType)
    {
        GetHandlePositionsForRectFrame(rect, out var handlePositions);
        EditDrawer!.DrawRect(rect, Colors.Black, false, 4 / _drawZoom);
        EditDrawer.DrawRect(rect, color, false, 2 / _drawZoom);

        var selectionHandleSize = selectHandle.GetSize() * 1.5f / _drawZoom;
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
            case Dragging.HandleTopLeft:
            case Dragging.HandleTop:
            case Dragging.HandleTopRight:
            case Dragging.HandleRight:
            case Dragging.HandleBottomRight:
            case Dragging.HandleBottom:
            case Dragging.HandleBottomLeft:
            case Dragging.HandleLeft:
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
