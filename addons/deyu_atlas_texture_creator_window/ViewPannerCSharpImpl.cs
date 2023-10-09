using System;
using Godot;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

/// <summary>
///     The C# Implementation of the original ViewPanner exists in godot source code (view_panner.cpp)
/// </summary>
public class ViewPannerCSharpImpl
{
    public enum ControlScheme { ScrollZooms, ScrollPans }

    public enum PanAxis { Both, Horizontal, Vertical }

    private bool m_IsDragging;

    private Action<Vector2> m_PanCallback;
    private bool m_PanKeyPressed;

    private float m_ScrollZoomFactor = 1.1f;
    private Action<float, Vector2> m_ZoomCallback;

    public bool IsPanning => m_IsDragging || m_PanKeyPressed;
    public bool ForceDrag { get; set; }
    public int ScrollSpeed { get; set; } = 32;
    public bool EnableRmb { get; set; }
    public bool SimplePanningEnabled { get; set; }
    public Shortcut PanViewShortcut { get; private set; }
    public ControlScheme CurrentControlScheme { get; set; } = ControlScheme.ScrollZooms;
    public PanAxis CurrentPanAxis { get; set; } = PanAxis.Both;


    public void SetCallbacks(Action<Vector2> panCallback, Action<float, Vector2> zoomCallback)
    {
        m_PanCallback = panCallback;
        m_ZoomCallback = zoomCallback;
    }

    public void SetPanShortcut(Shortcut p_shortcut)
    {
        PanViewShortcut = p_shortcut;
        m_PanKeyPressed = false;
    }

    public void SetScrollSpeed(int scrollSpeed)
    {
        if (scrollSpeed <= 0) throw new ArgumentOutOfRangeException(nameof(scrollSpeed), "p_scroll_speed <= 0");
        ScrollSpeed = scrollSpeed;
    }

    public void SetScrollZoomFactor(float scrollZoomFactor)
    {
        if (scrollZoomFactor <= 1.0) throw new ArgumentOutOfRangeException(nameof(scrollZoomFactor), "p_scroll_zoom_factor <= 1.0");
        m_ScrollZoomFactor = scrollZoomFactor;
    }

    public void Setup(ControlScheme controlScheme, Shortcut shortcut, bool simplePanning)
    {
        CurrentControlScheme = controlScheme;
        SetPanShortcut(shortcut);
        SimplePanningEnabled = simplePanning;
    }

    public bool ProcessGuiInput(InputEvent inputEvent, Rect2 canvasRect)
    {
        switch (inputEvent)
        {
            case InputEventMouseButton mb:
                Vector2 scroll_vec =
                    new(
                        Convert.ToInt32(mb.ButtonIndex == MouseButton.WheelRight) - Convert.ToInt32(mb.ButtonIndex == MouseButton.WheelLeft),
                        Convert.ToInt32(mb.ButtonIndex == MouseButton.WheelDown) - Convert.ToInt32(mb.ButtonIndex == MouseButton.WheelUp)
                    );

                // Moving the scroll wheel sends two events: one with pressed as true,
                // and one with pressed as false. Make sure we only process one of them.
                if (scroll_vec != Vector2.Zero && mb.IsPressed())
                {
                    if (CurrentControlScheme == ControlScheme.ScrollPans)
                    {
                        if (mb.IsCommandOrControlPressed())
                        {
                            // Compute the zoom factor.
                            var zoom = scroll_vec.X + scroll_vec.Y > 0 ? 1.0f / m_ScrollZoomFactor : m_ScrollZoomFactor;
                            m_ZoomCallback(zoom, mb.Position);
                            return true;
                        }

                        var panning = scroll_vec * mb.Factor;
                        switch (CurrentPanAxis)
                        {
                            case PanAxis.Horizontal:
                                panning = new(panning.X + panning.Y, 0);
                                break;
                            case PanAxis.Vertical:
                                panning = new(0, panning.X + panning.Y);
                                break;
                            case PanAxis.Both:
                            default:
                                if (mb.ShiftPressed) panning = new(panning.Y, panning.X);

                                break;
                        }

                        m_PanCallback(-panning * ScrollSpeed);
                        return true;
                    }

                    if (mb.IsCommandOrControlPressed())
                    {
                        var panning = scroll_vec * mb.Factor;
                        switch (CurrentPanAxis)
                        {
                            case PanAxis.Horizontal:
                                panning = new(panning.X + panning.Y, 0);
                                break;
                            case PanAxis.Vertical:
                                panning = new(0, panning.X + panning.Y);
                                break;
                            case PanAxis.Both:
                            default:
                                if (mb.ShiftPressed) panning = new(panning.Y, panning.X);

                                break;
                        }

                        m_PanCallback(-panning * ScrollSpeed);
                        return true;
                    }

                    if (!mb.ShiftPressed)
                    {
                        // Compute the zoom factor.
                        var zoom = scroll_vec.X + scroll_vec.Y > 0 ? 1.0f / m_ScrollZoomFactor : m_ScrollZoomFactor;
                        m_ZoomCallback(zoom, mb.Position);
                        return true;
                    }
                }

                // Alt is not used for button presses, so ignore it.
                if (mb.AltPressed) return false;

                var is_drag_event =
                    mb.ButtonIndex == MouseButton.Middle ||
                    EnableRmb && mb.ButtonIndex == MouseButton.Right ||
                    !SimplePanningEnabled && mb.ButtonIndex == MouseButton.Left && IsPanning ||
                    ForceDrag && mb.ButtonIndex == MouseButton.Left;

                if (is_drag_event)
                {
                    m_IsDragging = mb.IsPressed();
                    return mb.ButtonIndex != MouseButton.Left || mb.IsPressed(); // Don't consume LMB release events (it fixes some selection problems).
                }

                break;
            case InputEventMouseMotion mm when m_IsDragging:
                m_PanCallback(mm.Relative);
                if (canvasRect != new Rect2()) Input.WarpMouse(mm.Position);

                return true;
            case InputEventMagnifyGesture magnify_gesture:
                // Zoom gesture
                m_ZoomCallback(magnify_gesture.Factor, magnify_gesture.Position);
                return true;
            case InputEventPanGesture pan_gesture:
                m_PanCallback(-pan_gesture.Delta * ScrollSpeed);
                break;
            case InputEventScreenDrag screen_drag:
                // if (Input::get_singleton() . is_emulating_mouse_from_touch() || Input::get_singleton() . is_emulating_touch_from_mouse())
                // {
                //     // This set of events also generates/is generated by
                //     // InputEventMouseButton/InputEventMouseMotion events which will be processed instead.
                // }
                // else
                // {
                m_PanCallback(screen_drag.Relative);
                // }
                break;
            case InputEventKey k when PanViewShortcut.HasValidEvent() && PanViewShortcut.MatchesEvent(k):
                m_PanKeyPressed = k.IsPressed();
                if (SimplePanningEnabled || Input.GetMouseButtonMask().HasFlag(MouseButtonMask.Left)) m_IsDragging = m_PanKeyPressed;

                return true;
        }

        return false;
    }

    public void ReleasePanKey()
    {
        m_PanKeyPressed = false;
        m_IsDragging = false;
    }
}
