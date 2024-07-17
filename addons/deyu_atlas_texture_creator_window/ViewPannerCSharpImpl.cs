using System;
using Godot;

namespace GodotTextureSlicer;

/// <summary>
///     The C# Implementation of the original ViewPanner exists in godot source code (view_panner.cpp)
/// </summary>
class ViewPannerCSharpImpl
{
    public enum ControlScheme { ScrollZooms, ScrollPans }

    public enum PanAxis { Both, Horizontal, Vertical }

    private bool _isDragging;

    private readonly Action<Vector2> _panCallback;
    private bool _panKeyPressed;

    private float _scrollZoomFactor = 1.1f;
    private readonly Action<float, Vector2> _zoomCallback;

    public bool IsPanning => _isDragging || _panKeyPressed;
    public bool ForceDrag { get; set; }
    public int ScrollSpeed { get; set; } = 32;
    public bool EnableRmb { get; set; }
    public bool SimplePanningEnabled { get; set; }
    public Shortcut PanViewShortcut { get; private set; }
    public ControlScheme CurrentControlScheme { get; set; } = ControlScheme.ScrollZooms;
    public PanAxis CurrentPanAxis { get; set; } = PanAxis.Both;

    internal ViewPannerCSharpImpl(Action<Vector2> panCallback, Action<float, Vector2> zoomCallback, ControlScheme controlScheme, Shortcut shortcut, bool simplePanning)
    {
        _panCallback = panCallback;
        _zoomCallback = zoomCallback;
        PanViewShortcut = shortcut;
        _panKeyPressed = false;
        CurrentControlScheme = controlScheme;
        SimplePanningEnabled = simplePanning;
    }

    public void SetScrollSpeed(int scrollSpeed)
    {
        if (scrollSpeed <= 0) throw new ArgumentOutOfRangeException(nameof(scrollSpeed), "p_scroll_speed <= 0");
        ScrollSpeed = scrollSpeed;
    }

    public void SetScrollZoomFactor(float scrollZoomFactor)
    {
        if (scrollZoomFactor <= 1.0) throw new ArgumentOutOfRangeException(nameof(scrollZoomFactor), "p_scroll_zoo_factor <= 1.0");
        _scrollZoomFactor = scrollZoomFactor;
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
                            var zoom = scroll_vec.X + scroll_vec.Y > 0 ? 1.0f / _scrollZoomFactor : _scrollZoomFactor;
                            _zoomCallback(zoom, mb.Position);
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

                        _panCallback(-panning * ScrollSpeed);
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

                        _panCallback(-panning * ScrollSpeed);
                        return true;
                    }

                    if (!mb.ShiftPressed)
                    {
                        // Compute the zoom factor.
                        var zoom = scroll_vec.X + scroll_vec.Y > 0 ? 1.0f / _scrollZoomFactor : _scrollZoomFactor;
                        _zoomCallback(zoom, mb.Position);
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
                    _isDragging = mb.IsPressed();
                    return mb.ButtonIndex != MouseButton.Left || mb.IsPressed(); // Don't consume LMB release events (it fixes some selection problems).
                }

                break;
            case InputEventMouseMotion mm when _isDragging:
                _panCallback(mm.Relative);
                if (canvasRect != new Rect2()) Input.WarpMouse(mm.Position);

                return true;
            case InputEventMagnifyGesture magnify_gesture:
                // Zoom gesture
                _zoomCallback(magnify_gesture.Factor, magnify_gesture.Position);
                return true;
            case InputEventPanGesture pan_gesture:
                _panCallback(-pan_gesture.Delta * ScrollSpeed);
                break;
            case InputEventScreenDrag screen_drag:
                // if (Input::get_singleton() . is_emulating_mouse_fro_touch() || Input::get_singleton() . is_emulating_touch_fro_mouse())
                // {
                //     // This set of events also generates/is generated by
                //     // InputEventMouseButton/InputEventMouseMotion events which will be processed instead.
                // }
                // else
                // {
                _panCallback(screen_drag.Relative);
                // }
                break;
            case InputEventKey k when PanViewShortcut.HasValidEvent() && PanViewShortcut.MatchesEvent(k):
                _panKeyPressed = k.IsPressed();
                if (SimplePanningEnabled || Input.GetMouseButtonMask().HasFlag(MouseButtonMask.Left)) _isDragging = _panKeyPressed;

                return true;
        }

        return false;
    }

    public void ReleasePanKey()
    {
        _panKeyPressed = false;
        _isDragging = false;
    }
}
