#if TOOLS

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace GodotTextureSlicer;
// This script contains the exports and api used by the Save & Discard Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    private readonly List<Rect2> _slicePreview = new();


    private SliceMethod _currentSlicerMode = SliceMethod.Automatic;

    [Export, ExportSubgroup("Slicer Subview Section")] private Control? SlicerMenu { get; set; }

    [Export] private OptionButton? SlicerTypeSelection { get; set; }
    [Export] private OptionButton? PreservationMethodSelection { get; set; }
    [Export] private Button? ExecuteSliceButton { get; set; }

    [Export, ExportSubgroup("Slicer Subview Section/Defaults")] private SpinBox? NewAtlasTextureMarginXInput { get; set; }

    [Export] private SpinBox? NewAtlasTextureMarginYInput { get; set; }
    [Export] private SpinBox? NewAtlasTextureMarginWInput { get; set; }
    [Export] private SpinBox? NewAtlasTextureMarginHInput { get; set; }
    [Export] private CheckBox? FilterClip { get; set; }

    [Export, ExportSubgroup("Slicer Subview Section/SliceMode - CellSize")]
    private Control[]? CellSizeGroup { get; set; }

    [Export] private SpinBox? CellSizePixelSizeX { get; set; }
    [Export] private SpinBox? CellSizePixelSizeY { get; set; }
    [Export] private SpinBox? CellSizeOffsetX { get; set; }
    [Export] private SpinBox? CellSizeOffsetY { get; set; }
    [Export] private SpinBox? CellSizePaddingX { get; set; }
    [Export] private SpinBox? CellSizePaddingY { get; set; }
    [Export] private CheckBox? CellSizeKeepEmptyRects { get; set; }

    [Export, ExportSubgroup("Slicer Subview Section/SliceMode - CellCount")]
    private Control[]? CellCountGroup { get; set; }

    [Export] private SpinBox? CellCountColumnRowX { get; set; }
    [Export] private SpinBox? CellCountColumnRowY { get; set; }
    [Export] private SpinBox? CellCountOffsetX { get; set; }
    [Export] private SpinBox? CellCountOffsetY { get; set; }
    [Export] private SpinBox? CellCountPaddingX { get; set; }
    [Export] private SpinBox? CellCountPaddingY { get; set; }
    [Export] private CheckBox? CellCountKeepEmptyRects { get; set; }

    private SliceMethod CurrentSlicerMode
    {
        get => _currentSlicerMode;
        set
        {
            _currentSlicerMode = value;
            switch (_currentSlicerMode)
            {
                case SliceMethod.Automatic:
                    foreach (var control in CellSizeGroup!)
                    {
                        control.Hide();
                    }

                    foreach (var control in CellCountGroup!)
                    {
                        control.Hide();
                    }

                    break;
                case SliceMethod.GridByCellSize:
                    foreach (var control in CellCountGroup!)
                    {
                        control.Hide();
                    }

                    foreach (var control in CellSizeGroup!)
                    {
                        control.Show();
                    }

                    break;
                case SliceMethod.GridByCellCount:
                    foreach (var control in CellSizeGroup!)
                    {
                        control.Hide();
                    }

                    foreach (var control in CellCountGroup!)
                    {
                        control.Show();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            PreviewCurrentSlice();
        }
    }

    private PreservationMethod CurrentPreservationMethod { get; set; }

    private enum SliceMethod { Automatic = 0, GridByCellSize = 1, GridByCellCount = 2 }

    private enum PreservationMethod { IgnoreExisting = 0, AvoidExisting = 1 }

    /// <summary>
    ///     Initialize the slicer module with the given settings
    /// </summary>
    private void InitializeSlicer(EditorSettings settings)
    {
        RegButtonToggled(
            AtlasTextureSlicerButton!,
            isOn =>
            {
                if (_inspectingTex is null) return;

                if (isOn) ShowSlicerMenu();
                else HideSlicerMenu();
            }
        );


        CurrentSlicerMode =
            settings
               .GetProjectMetadata(
                    "atlas_texture_editor",
                    "slice_mode",
                    Variant.From(SliceMethod.Automatic)
                )
               .As<SliceMethod>();

        CurrentPreservationMethod =
            settings.GetProjectMetadata(
                         "atlas_texture_editor",
                         "preservation_mode",
                         Variant.From(PreservationMethod.AvoidExisting)
                     )
                    .As<PreservationMethod>();


        SlicerTypeSelection!.AddItem("Automatic", (int)SliceMethod.Automatic);
        SlicerTypeSelection.AddItem("Grid By Cell Size", (int)SliceMethod.GridByCellSize);
        SlicerTypeSelection.AddItem("Grid By Cell Count", (int)SliceMethod.GridByCellCount);
        RegOptionButtonItemSelected(SlicerTypeSelection, pMode => CurrentSlicerMode = (SliceMethod)pMode);
        SlicerTypeSelection.Selected = (int)CurrentSlicerMode;

        PreservationMethodSelection!.AddItem("Ignore Existing (Additive)", (int)PreservationMethod.IgnoreExisting);
        PreservationMethodSelection.AddItem("Avoid Existing (Smart)", (int)PreservationMethod.AvoidExisting);
        RegOptionButtonItemSelected(PreservationMethodSelection, mode => CurrentPreservationMethod = (PreservationMethod)mode);
        PreservationMethodSelection.Selected = (int)CurrentPreservationMethod;

        ExecuteSliceButton!.Pressed += PerformSlice;

        RegRangeValueChanged(CellSizePixelSizeX!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSizePixelSizeY!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSizeOffsetX!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSizeOffsetY!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSizePaddingX!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSizePaddingY!, PreviewCurrentSliceDeferred);
        RegButtonToggled(CellSizeKeepEmptyRects!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCountColumnRowX!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCountColumnRowY!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCountOffsetX!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCountOffsetY!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCountPaddingX!, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCountPaddingY!, PreviewCurrentSliceDeferred);
        RegButtonToggled(CellCountKeepEmptyRects!, PreviewCurrentSliceDeferred);

        SlicerMenu!.Hide();
    }

    private void PreviewCurrentSliceDeferred(double newValue) => CallDeferred(MethodName.PreviewCurrentSlice);
    private void PreviewCurrentSliceDeferred(bool newValue) => CallDeferred(MethodName.PreviewCurrentSlice);

    /// <summary>
    ///     Show the slicer menu and trigger a preview of the current slice
    /// </summary>
    private void ShowSlicerMenu()
    {
        _slicePreview.Clear();
        SlicerMenu!.Show();
        PreviewCurrentSlice();
        EditDrawer!.QueueRedraw();
    }

    /// <summary>
    ///     Hide the slicer menu and clear the slice preview
    /// </summary>
    private void HideSlicerMenu()
    {
        _slicePreview.Clear();
        SlicerMenu!.Hide();
        EditDrawer!.QueueRedraw();
    }

    /// <summary>
    ///     Perform the selected slicing mode (<see cref="CurrentSlicerMode" />), and creates the corresponding
    ///     <see cref="EditingAtlasTextureInfo" /> into <see cref="_editingAtlasTexture" />
    /// </summary>
    private void PerformSlice()
    {
        _slicePreview.Clear();

        switch (CurrentSlicerMode)
        {
            case SliceMethod.Automatic:
                CalculateAutomaticSlice(_inspectingTex!, _slicePreview);
                break;
            case SliceMethod.GridByCellSize:
                CalculateByCellSizeSlice(
                    _inspectingTex!,
                    _slicePreview,
                    new(Mathf.RoundToInt(CellSizePixelSizeX!.Value), Mathf.RoundToInt(CellSizePixelSizeY!.Value)),
                    new((float)CellSizeOffsetX!.Value, (float)CellSizeOffsetY!.Value),
                    new((float)CellSizePaddingX!.Value, (float)CellSizePaddingY!.Value),
                    CellSizeKeepEmptyRects!.ButtonPressed
                );
                break;
            case SliceMethod.GridByCellCount:
                CalculateByCellCountSlice(
                    _inspectingTex!,
                    _slicePreview,
                    new(Mathf.RoundToInt(CellCountColumnRowX!.Value), Mathf.RoundToInt(CellCountColumnRowY!.Value)),
                    new((float)CellCountOffsetX!.Value, (float)CellCountOffsetY!.Value),
                    new((float)CellCountPaddingX!.Value, (float)CellCountPaddingY!.Value),
                    CellCountKeepEmptyRects!.ButtonPressed
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (CurrentPreservationMethod is PreservationMethod.AvoidExisting)
            for (var i = 0; i < _slicePreview.Count; i++)
            {
                var current = _slicePreview[i];

                if (!_editingAtlasTexture.Any(editingAtlasTextureInfo => editingAtlasTextureInfo.Region.Intersects(current))) continue;

                _slicePreview.RemoveAt(i);
                i--;
            }

        var filterClip = FilterClip!.ButtonPressed;

        var margin = new Rect2(
            (float)NewAtlasTextureMarginXInput!.Value,
            (float)NewAtlasTextureMarginYInput!.Value,
            (float)NewAtlasTextureMarginWInput!.Value,
            (float)NewAtlasTextureMarginHInput!.Value
        );

        foreach (var slice in _slicePreview)
        {
            CreateSlice(slice, margin, filterClip);
        }

        _slicePreview.Clear();
        UpdateControls();
    }

    /// <summary>
    ///     Creates a preview for current selecting slicing mode (<see cref="CurrentSlicerMode" />)
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void PreviewCurrentSlice()
    {
        _slicePreview.Clear();
        switch (CurrentSlicerMode)
        {
            case SliceMethod.Automatic:
                break;
            case SliceMethod.GridByCellSize:
                CalculateByCellSizeSlice(
                    _inspectingTex!,
                    _slicePreview,
                    new(Mathf.RoundToInt(CellSizePixelSizeX!.Value), Mathf.RoundToInt(CellSizePixelSizeY!.Value)),
                    new((float)CellSizeOffsetX!.Value, (float)CellSizeOffsetY!.Value),
                    new((float)CellSizePaddingX!.Value, (float)CellSizePaddingY!.Value),
                    true
                );
                break;
            case SliceMethod.GridByCellCount:
                CalculateByCellCountSlice(
                    _inspectingTex!,
                    _slicePreview,
                    new(Mathf.RoundToInt(CellCountColumnRowX!.Value), Mathf.RoundToInt(CellCountColumnRowY!.Value)),
                    new((float)CellCountOffsetX!.Value, (float)CellCountOffsetY!.Value),
                    new((float)CellCountPaddingX!.Value, (float)CellCountPaddingY!.Value),
                    true
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        EditDrawer!.QueueRedraw();
    }

    private static bool[,] CreateAlphaMask(Image image, out int width, out int height)
    {
        if (image.IsCompressed())
        {
            var decompressed = (Image)image.Duplicate();
            decompressed.Decompress();
            image = decompressed;
        }

        var bitmap = new Bitmap();
        bitmap.CreateFromImageAlpha(image, 0f);
        (width, height) = bitmap.GetSize();
        
        var bitmask = bitmap.ConvertToImage().GetData()!;
        
        (width, height) = bitmap.GetSize();
        
        var mask = new bool[width, height];
        
        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var index = width * y + x;
            var value = bitmask[index] != 0;
            
            mask[x, y] = value;
        }

        
        return mask;
    }

    /// <summary>
    ///     Core method for Automatic Slice Calculation
    /// </summary>
    private static void CalculateAutomaticSlice(Texture2D texture, IList<Rect2> sliceData, bool mergeRects=true)
    {
        sliceData.Clear();
        
        var mask = new Bitmap();
        mask.CreateFromImageAlpha(texture.GetImage());
        
        var maskRect = new Rect2I(Vector2I.Zero, mask.GetSize());

        var polygons = mask.OpaqueToPolygons(maskRect, 0.0f);
        
        foreach (var polygon in polygons)
        {
            var rect = new Rect2(polygon.First(), Vector2.Zero);
            for (int i = 1; i < polygon.Length; i++)
            {
                rect = rect.Expand(polygon[i]);
            }

            if (!mergeRects)
            {
                sliceData.Add(rect);
            }
            else
            {
                var intersected = false;
                for (var index = 0; index < sliceData.Count; index++)
                {
                    var existSlice = sliceData[index];
                    if (!rect.Intersects(existSlice)) continue;
                    sliceData[index] = existSlice.Merge(rect);
                    intersected = true;
                    break;
                }
                if (!intersected) sliceData.Add(rect);
            }
        }
    }

    /// <summary>
    ///     Core method for Slice Calculation based on Cell Size
    /// </summary>
    private static void CalculateByCellSizeSlice(Texture2D texture, IList<Rect2> sliceData, Vector2 pixelSize, Vector2 offset, Vector2 margin, bool keepEmptyRect)
    {
        var (textureWidth, textureHeight) = texture.GetSize();
        pixelSize = new(Mathf.Max(pixelSize.X, 1), Mathf.Max(pixelSize.Y, 1));
        for (var y = offset.Y; y < textureHeight; y = y + pixelSize.Y + margin.Y)
        {
            for (var x = offset.X; x < textureWidth; x = x + pixelSize.X + margin.X)
            {
                sliceData.Add(new(x, y, pixelSize));
            }
        }

        if (keepEmptyRect) return;

        for (var index = 0; index < sliceData.Count; index++)
        {
            var sliceRect = sliceData[index];
            var valid = false;
            for (var y = sliceRect.Position.Y; y < sliceRect.End.Y; y++)
            {
                for (var x = sliceRect.Position.X; x < sliceRect.End.X; x++)
                {
                    if (x >= textureWidth || y >= textureHeight || !IsPixelOpaqueImpl(texture, Mathf.RoundToInt(x), Mathf.RoundToInt(y))) continue;

                    valid = true;
                    break;
                }
            }

            if (valid) continue;

            sliceData.RemoveAt(index);
            index--;
        }
    }

    /// <summary>
    ///     Core method for Slice Calculation based on Cell Count
    /// </summary>
    private static void CalculateByCellCountSlice(Texture2D texture, IList<Rect2> sliceData, Vector2I columnRow, Vector2 offset, Vector2 margin, bool keepEmptyRect)
    {
        var textureMetrics = texture.GetSize();
        var calculatedPixelSize = textureMetrics / new Vector2I(Mathf.Max(columnRow.X, 1), Mathf.Max(columnRow.Y, 1));
        CalculateByCellSizeSlice(texture, sliceData, calculatedPixelSize, offset, margin, keepEmptyRect);
    }
}

#endif
