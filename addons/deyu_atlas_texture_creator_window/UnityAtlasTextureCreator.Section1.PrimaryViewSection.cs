#if TOOLS

#region

using System;
using Godot;

#endregion

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

// This script contains the exports and api used by the Primary View Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    private enum Dragging
    {
        None = -1,
        Area = -2,
        Handle_TopLeft = 0,
        Handle_Top = 1,
        Handle_TopRight = 2,
        Handle_Right = 3,
        Handle_BottomRight = 4,
        Handle_Bottom = 5,
        Handle_BottomLeft = 6,
        Handle_Left = 7
    }
    
    [Export, ExportSubgroup("Primary View Section")] private Control EditDrawer { get; set; }
    [Export] private Button ZoomInButton { get; set; }
    [Export] private Button ZoomResetButton { get; set; }
    [Export] private Button ZoomOutButton { get; set; }
    [Export] private VScrollBar VScroll { get; set; }
    [Export] private HScrollBar HScroll { get; set; }

    /// <summary>
    /// Initialize the primary view section with editor settings
    /// </summary>
    /// <param name="settings"></param>
    private void InitializePrimaryViewSection(EditorSettings settings)
    {
        m_PreviewTex = new();

        m_ViewPanner = new();
        m_ViewPanner.SetCallbacks(
            Pan,
            (p_zoom_factor, p_origin, _) => ZoomOnPosition(m_DrawZoom * p_zoom_factor, p_origin)
        );

        EditDrawer.Draw += DrawRegion;
        EditDrawer.GuiInput += InputRegion;
        EditDrawer.FocusExited += m_ViewPanner.ReleasePanKey;

        m_DrawZoom = 1.0f;

        BindZoomButtons(
            ZoomInButton,
            "Zoom Out",
            () => ZoomOnPosition(m_DrawZoom / 1.5f, EditDrawer.Size / 2.0f),
            "ZoomLess"
        );
        BindZoomButtons(
            ZoomResetButton,
            "Zoom Reset",
            () => ZoomOnPosition(1.0f, EditDrawer.Size / 2.0f),
            "ZoomReset"
        );
        BindZoomButtons(
            ZoomOutButton,
            "Zoom In",
            () => ZoomOnPosition(m_DrawZoom * 1.5f, EditDrawer.Size / 2.0f),
            "ZoomMore"
        );

        VScroll.ValueChanged += OnScrollChanged;
        HScroll.ValueChanged += OnScrollChanged;

        void OnScrollChanged(double _)
        {
            if (m_UpdatingScroll) return;

            m_DrawOffsets.X = (float)HScroll.Value;
            m_DrawOffsets.Y = (float)VScroll.Value;
            EditDrawer.QueueRedraw();
        }

        EditDrawer.AddThemeStyleboxOverride("panel", Theme.GetStylebox("panel", "Tree"));
        m_ViewPanner.Setup(
            settings.Get("editors/panning/sub_editors_panning_scheme").As<ViewPannerCSharpImpl.ControlScheme>(),
            new(), // settings.GetShortcut("canvas_item_editor/pan_view"); // This api only exists in native side, Sad :(
            settings.Get("editors/panning/simple_panning").As<bool>()
        );

        m_UpdatingScroll = false;

        void BindZoomButtons(Button button, string text, Action onPress, string editorIconName)
        {
            button.Flat = true;
            button.TooltipText = Tr(text);
            button.Icon = Theme.GetIcon(editorIconName, "EditorIcons");
            button.Pressed += onPress;
        }
    }

    /// <summary>
    /// Calculate eight handle positions for a given rectangle frame
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

        m_HandlePositionBuffer[0] = rawEndpoints0_handle1;
        m_HandlePositionBuffer[1] = rawEndpoints0_handle2;
        m_HandlePositionBuffer[2] = rawEndpoints1_handle1;
        m_HandlePositionBuffer[3] = rawEndpoints1_handle2;
        m_HandlePositionBuffer[4] = rawEndpoints2_handle1;
        m_HandlePositionBuffer[5] = rawEndpoints2_handle2;
        m_HandlePositionBuffer[6] = rawEndpoints3_handle1;
        m_HandlePositionBuffer[7] = rawEndpoints3_handle2;

        eightHandlePositions = m_HandlePositionBuffer.AsSpan();

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
                m_DrawZoom;

            handle1 = position + offset;

            offset = (nextPosition - position) / 2;
            offset +=
                (nextPosition - position)
               .Orthogonal()
               .Normalized() *
                10f /
                m_DrawZoom;

            handle2 = position + offset;
        }
    }

    /// <summary>
    /// Zoom the view at a specific position
    /// </summary>
    private void ZoomOnPosition(float p_zoom, Vector2 p_position)
    {
        if (p_zoom < 0.25 || p_zoom > 8) return;

        var prev_zoom = m_DrawZoom;
        m_DrawZoom = p_zoom;
        var ofs = p_position;
        ofs = ofs / prev_zoom - ofs / m_DrawZoom;
        m_DrawOffsets = (m_DrawOffsets + ofs).Round();

        EditDrawer.QueueRedraw();
    }
}

#endif