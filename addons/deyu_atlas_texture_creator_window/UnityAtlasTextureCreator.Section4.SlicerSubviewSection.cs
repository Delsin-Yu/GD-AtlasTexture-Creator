#if TOOLS

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;
// This script contains the exports and api used by the Save & Discard Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    private readonly List<Rect2> m_SlicePreview = new();


    private SliceMethod m_CurrentSlicerMode = SliceMethod.Automatic;

    [Export, ExportSubgroup("Slicer Subview Section")] private Control SlicerMenu { get; set; }

    [Export] private OptionButton SlicerTypeSelection { get; set; }
    [Export] private OptionButton PreservationMethodSelection { get; set; }
    [Export] private Button ExecuteSliceButton { get; set; }

    [Export, ExportSubgroup("Slicer Subview Section/Defaults")] private SpinBox NewAtlasTextureMarginXInput { get; set; }

    [Export] private SpinBox NewAtlasTextureMarginYInput { get; set; }
    [Export] private SpinBox NewAtlasTextureMarginWInput { get; set; }
    [Export] private SpinBox NewAtlasTextureMarginHInput { get; set; }
    [Export] private CheckBox FilterClip { get; set; }

    [Export, ExportSubgroup("Slicer Subview Section/SliceMode - CellSize")]
    private Control[] CellSizeGroup { get; set; }

    [Export] private SpinBox CellSize_PixelSizeX { get; set; }
    [Export] private SpinBox CellSize_PixelSizeY { get; set; }
    [Export] private SpinBox CellSize_OffsetX { get; set; }
    [Export] private SpinBox CellSize_OffsetY { get; set; }
    [Export] private SpinBox CellSize_PaddingX { get; set; }
    [Export] private SpinBox CellSize_PaddingY { get; set; }
    [Export] private CheckBox CellSize_KeepEmptyRects { get; set; }

    [Export, ExportSubgroup("Slicer Subview Section/SliceMode - CellCount")]
    private Control[] CellCountGroup { get; set; }

    [Export] private SpinBox CellCount_ColumnRowX { get; set; }
    [Export] private SpinBox CellCount_ColumnRowY { get; set; }
    [Export] private SpinBox CellCount_OffsetX { get; set; }
    [Export] private SpinBox CellCount_OffsetY { get; set; }
    [Export] private SpinBox CellCount_PaddingX { get; set; }
    [Export] private SpinBox CellCount_PaddingY { get; set; }
    [Export] private CheckBox CellCount_KeepEmptyRects { get; set; }

    private SliceMethod CurrentSlicerMode
    {
        get => m_CurrentSlicerMode;
        set
        {
            m_CurrentSlicerMode = value;
            switch (m_CurrentSlicerMode)
            {
                case SliceMethod.Automatic:
                    foreach (var control in CellSizeGroup)
                    {
                        control.Hide();
                    }

                    foreach (var control in CellCountGroup)
                    {
                        control.Hide();
                    }

                    break;
                case SliceMethod.GridByCellSize:
                    foreach (var control in CellCountGroup)
                    {
                        control.Hide();
                    }

                    foreach (var control in CellSizeGroup)
                    {
                        control.Show();
                    }

                    break;
                case SliceMethod.GridByCellCount:
                    foreach (var control in CellSizeGroup)
                    {
                        control.Hide();
                    }

                    foreach (var control in CellCountGroup)
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
            AtlasTextureSlicerButton,
            isOn =>
            {
                if (m_InspectingTex is null) return;

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


        SlicerTypeSelection.AddItem("Automatic", (int)SliceMethod.Automatic);
        SlicerTypeSelection.AddItem("Grid By Cell Size", (int)SliceMethod.GridByCellSize);
        SlicerTypeSelection.AddItem("Grid By Cell Count", (int)SliceMethod.GridByCellCount);
        RegOptionButtonItemSelected(SlicerTypeSelection, p_mode => CurrentSlicerMode = (SliceMethod)p_mode);
        SlicerTypeSelection.Selected = (int)CurrentSlicerMode;

        PreservationMethodSelection.AddItem("Ignore Existing (Additive)", (int)PreservationMethod.IgnoreExisting);
        PreservationMethodSelection.AddItem("Avoid Existing (Smart)", (int)PreservationMethod.AvoidExisting);
        RegOptionButtonItemSelected(PreservationMethodSelection, mode => CurrentPreservationMethod = (PreservationMethod)mode);
        PreservationMethodSelection.Selected = (int)CurrentPreservationMethod;

        ExecuteSliceButton.Pressed += PerformSlice;

        RegRangeValueChanged(CellSize_PixelSizeX, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSize_PixelSizeY, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSize_OffsetX, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSize_OffsetY, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSize_PaddingX, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellSize_PaddingY, PreviewCurrentSliceDeferred);
        RegButtonToggled(CellSize_KeepEmptyRects, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCount_ColumnRowX, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCount_ColumnRowY, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCount_OffsetX, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCount_OffsetY, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCount_PaddingX, PreviewCurrentSliceDeferred);
        RegRangeValueChanged(CellCount_PaddingY, PreviewCurrentSliceDeferred);
        RegButtonToggled(CellCount_KeepEmptyRects, PreviewCurrentSliceDeferred);

        SlicerMenu.Hide();
    }

    private void PreviewCurrentSliceDeferred(double newValue) => CallDeferred(MethodName.PreviewCurrentSlice);
    private void PreviewCurrentSliceDeferred(bool newValue) => CallDeferred(MethodName.PreviewCurrentSlice);

    /// <summary>
    ///     Show the slicer menu and trigger a preview of the current slice
    /// </summary>
    private void ShowSlicerMenu()
    {
        m_SlicePreview.Clear();
        SlicerMenu.Show();
        PreviewCurrentSlice();
        EditDrawer.QueueRedraw();
    }

    /// <summary>
    ///     Hide the slicer menu and clear the slice preview
    /// </summary>
    private void HideSlicerMenu()
    {
        m_SlicePreview.Clear();
        SlicerMenu.Hide();
        EditDrawer.QueueRedraw();
    }

    /// <summary>
    ///     Perform the selected slicing mode (<see cref="CurrentSlicerMode" />), and creates the corresponding
    ///     <see cref="EditingAtlasTextureInfo" /> into <see cref="m_EditingAtlasTexture" />
    /// </summary>
    private void PerformSlice()
    {
        m_SlicePreview.Clear();

        switch (CurrentSlicerMode)
        {
            case SliceMethod.Automatic:
                CalculateAutomaticSlice(m_InspectingTex, m_SlicePreview);
                break;
            case SliceMethod.GridByCellSize:
                CalculateByCellSizeSlice(
                    m_InspectingTex,
                    m_SlicePreview,
                    new(Mathf.RoundToInt(CellSize_PixelSizeX.Value), Mathf.RoundToInt(CellSize_PixelSizeY.Value)),
                    new((float)CellSize_OffsetX.Value, (float)CellSize_OffsetY.Value),
                    new((float)CellSize_PaddingX.Value, (float)CellSize_PaddingY.Value),
                    CellSize_KeepEmptyRects.ButtonPressed
                );
                break;
            case SliceMethod.GridByCellCount:
                CalculateByCellCountSlice(
                    m_InspectingTex,
                    m_SlicePreview,
                    new(Mathf.RoundToInt(CellCount_ColumnRowX.Value), Mathf.RoundToInt(CellCount_ColumnRowY.Value)),
                    new((float)CellCount_OffsetX.Value, (float)CellCount_OffsetY.Value),
                    new((float)CellCount_PaddingX.Value, (float)CellCount_PaddingY.Value),
                    CellCount_KeepEmptyRects.ButtonPressed
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (CurrentPreservationMethod is PreservationMethod.AvoidExisting)
            for (var i = 0; i < m_SlicePreview.Count; i++)
            {
                var current = m_SlicePreview[i];

                if (!m_EditingAtlasTexture.Any(editingAtlasTextureInfo => editingAtlasTextureInfo.Region.Intersects(current))) continue;

                m_SlicePreview.RemoveAt(i);
                i--;
            }

        var filterClip = FilterClip.ButtonPressed;

        var margin = new Rect2(
            (float)NewAtlasTextureMarginXInput.Value,
            (float)NewAtlasTextureMarginYInput.Value,
            (float)NewAtlasTextureMarginWInput.Value,
            (float)NewAtlasTextureMarginHInput.Value
        );

        foreach (var slice in m_SlicePreview)
        {
            CreateSlice(slice, margin, filterClip);
        }

        m_SlicePreview.Clear();
        UpdateControls();
    }

    /// <summary>
    ///     Creates a preview for current selecting slicing mode (<see cref="CurrentSlicerMode" />)
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    private void PreviewCurrentSlice()
    {
        m_SlicePreview.Clear();
        switch (CurrentSlicerMode)
        {
            case SliceMethod.Automatic:
                break;
            case SliceMethod.GridByCellSize:
                CalculateByCellSizeSlice(
                    m_InspectingTex,
                    m_SlicePreview,
                    new(Mathf.RoundToInt(CellSize_PixelSizeX.Value), Mathf.RoundToInt(CellSize_PixelSizeY.Value)),
                    new((float)CellSize_OffsetX.Value, (float)CellSize_OffsetY.Value),
                    new((float)CellSize_PaddingX.Value, (float)CellSize_PaddingY.Value),
                    true
                );
                break;
            case SliceMethod.GridByCellCount:
                CalculateByCellCountSlice(
                    m_InspectingTex,
                    m_SlicePreview,
                    new(Mathf.RoundToInt(CellCount_ColumnRowX.Value), Mathf.RoundToInt(CellCount_ColumnRowY.Value)),
                    new((float)CellCount_OffsetX.Value, (float)CellCount_OffsetY.Value),
                    new((float)CellCount_PaddingX.Value, (float)CellCount_PaddingY.Value),
                    true
                );
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        EditDrawer.QueueRedraw();
    }

    /// <summary>
    ///     Core method for Automatic Slice Calculation
    /// </summary>
    private static void CalculateAutomaticSlice(Texture2D texture, IList<Rect2> sliceData)
    {
        if (texture is null) return;

        sliceData.Clear();

        var (textureWidth, textureHeight) = texture.GetSize();

        for (var y = 0; y < textureHeight; y++)
        {
            for (var x = 0; x < textureWidth; x++)
            {
                if (!IsPixelOpaqueImpl(texture, x, y)) continue;
                var found = false;
                foreach (var e in ListItemReference<Rect2>.CreateForEach(sliceData))
                {
                    var grown = e.Value.Grow(1.5f);
                    if (!grown.HasPoint(new(x, y))) continue;
                    e.Value = e.Value.Expand(new(x, y));
                    e.Value = e.Value.Expand(new(x + 1, y + 1));
                    x = (int)(e.Value.Position.X + e.Value.Size.X - 1);
                    var merged = true;
                    while (merged)
                    {
                        merged = false;
                        var queue_erase = false;
                        for (var f = ListItemReference<Rect2>.CreateFor(sliceData); f.IsValid; f = f.GetNext())
                        {
                            if (queue_erase)
                            {
                                var prev = f.GetPrev();
                                if (prev.IsValid) sliceData.Remove(prev.Value);
                                queue_erase = false;
                            }

                            if (!f.IsValid || !e.IsValid) break;

                            if (f.Value == e.Value) continue;

                            if (!e.Value.Grow(1).Intersects(f.Value)) continue;
                            e.Value = e.Value.Expand(f.Value.Position);
                            e.Value = e.Value.Expand(f.Value.Position + f.Value.Size);
                            var prevF = f.GetPrev();
                            if (prevF.IsValid)
                            {
                                f = prevF;
                                var nextF = f.GetNext();
                                if (nextF.IsValid) sliceData.Remove(nextF.Value);
                            }
                            else queue_erase = true;

                            // Can't delete the first rect in the list.
                            merged = true;
                        }
                    }

                    found = true;
                    break;
                }

                if (found) continue;
                var new_rect = new Rect2(x, y, 1, 1);
                sliceData.Add(new_rect);
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
