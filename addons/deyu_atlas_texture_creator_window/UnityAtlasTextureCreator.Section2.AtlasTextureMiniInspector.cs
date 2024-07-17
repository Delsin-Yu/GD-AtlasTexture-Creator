#if TOOLS


using Godot;

namespace GodotTextureSlicer;
// This script contains the exports and api used by the AtlasTexture Mini Inspector Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    [Export, ExportSubgroup("AtlasTexture Mini Inspector Section")] private Control? MiniInspectorWindow { get; set; }

    [Export] private Label? NewItemLabel { get; set; }
    [Export] private Button? DeleteItemButton { get; set; }

    [Export, ExportSubgroup("AtlasTexture Mini Inspector Section/Inputs")] private LineEdit? AtlasTextureNameInput { get; set; }

    [Export, ExportSubgroup("AtlasTexture Mini Inspector Section/Inputs/Region")]
    private SpinBox? AtlasTextureRegionXInput { get; set; }

    [Export] private SpinBox? AtlasTextureRegionYInput { get; set; }
    [Export] private SpinBox? AtlasTextureRegionWInput { get; set; }
    [Export] private SpinBox? AtlasTextureRegionHInput { get; set; }

    [Export, ExportSubgroup("AtlasTexture Mini Inspector Section/Inputs/Margin")]
    private SpinBox? AtlasTextureMarginXInput { get; set; }

    [Export] private SpinBox? AtlasTextureMarginYInput { get; set; }
    [Export] private SpinBox? AtlasTextureMarginWInput { get; set; }
    [Export] private SpinBox? AtlasTextureMarginHInput { get; set; }
    [Export] private CheckBox? AtlasTextureFilterClipInput { get; set; }

    /// <summary>
    ///     Initialize callbacks for the MiniInspector
    /// </summary>
    private void InitializeAtlasTextureMiniInspector()
    {
        RegLineEdit(
            AtlasTextureNameInput!,
            newText =>
            {
                if (_inspectingAtlasTextureInfo is null || !_inspectingAtlasTextureInfo.IsTemp) return;
                if (_inspectingAtlasTextureInfo.TrySetName(newText)) UpdateControls();
            }
        );

        RegRangeValueChanged(
            AtlasTextureRegionXInput!,
            newRegionX =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                var value = _inspectingAtlasTextureInfo.Region;
                value.Position = new((float)newRegionX, value.Position.Y);
                if (_inspectingAtlasTextureInfo.TrySetRegion(value)) UpdateControls();
            }
        );
        RegRangeValueChanged(
            AtlasTextureRegionYInput!,
            newRegionY =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                var value = _inspectingAtlasTextureInfo.Region;
                value.Position = new(value.Position.X, (float)newRegionY);
                if (_inspectingAtlasTextureInfo.TrySetRegion(value)) UpdateControls();
            }
        );
        RegRangeValueChanged(
            AtlasTextureRegionWInput!,
            newRegionW =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                var value = _inspectingAtlasTextureInfo.Region;
                value.Size = new((float)newRegionW, value.Size.Y);
                if (_inspectingAtlasTextureInfo.TrySetRegion(value)) UpdateControls();
            }
        );
        RegRangeValueChanged(
            AtlasTextureRegionHInput!,
            newRegionH =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                var value = _inspectingAtlasTextureInfo.Region;
                value.Size = new(value.Size.X, (float)newRegionH);
                if (_inspectingAtlasTextureInfo.TrySetRegion(value)) UpdateControls();
            }
        );

        RegRangeValueChanged(
            AtlasTextureMarginXInput!,
            newMarginX =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                var value = _inspectingAtlasTextureInfo.Margin;
                value.Position = new((float)newMarginX, value.Position.Y);
                if (_inspectingAtlasTextureInfo.TrySetMargin(value)) UpdateControls();
            }
        );
        RegRangeValueChanged(
            AtlasTextureMarginYInput!,
            newMarginY =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                var value = _inspectingAtlasTextureInfo.Margin;
                value.Position = new(value.Position.X, (float)newMarginY);
                if (_inspectingAtlasTextureInfo.TrySetMargin(value)) UpdateControls();
            }
        );
        RegRangeValueChanged(
            AtlasTextureMarginWInput!,
            newMarginW =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                var value = _inspectingAtlasTextureInfo.Margin;
                value.Size = new((float)newMarginW, value.Size.Y);
                if (_inspectingAtlasTextureInfo.TrySetMargin(value)) UpdateControls();
            }
        );
        RegRangeValueChanged(
            AtlasTextureMarginHInput!,
            newMarginH =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                var value = _inspectingAtlasTextureInfo.Margin;
                value.Size = new(value.Size.X, (float)newMarginH);
                if (_inspectingAtlasTextureInfo.TrySetMargin(value)) UpdateControls();
            }
        );
        RegButtonToggled(
            AtlasTextureFilterClipInput!,
            newFilterClip =>
            {
                if (_inspectingAtlasTextureInfo is null) return;
                if (_inspectingAtlasTextureInfo.TrySetFilterClip(newFilterClip)) UpdateControls();
            }
        );
        RegButtonPressed(
            DeleteItemButton!,
            () =>
            {
                if (_inspectingAtlasTextureInfo is null || !_inspectingAtlasTextureInfo.IsTemp) return;
                _editingAtlasTexture.Remove(_inspectingAtlasTextureInfo);
                _inspectingAtlasTextureInfo = null;
                UpdateControls();
            }
        );
    }

    /// <summary>
    ///     Set the spin box mode (rounded or not) for all value input related controls
    /// </summary>
    /// <param name="rounded"></param>
    private void SetSpinBoxMode(bool rounded)
    {
        SetParams(AtlasTextureRegionXInput!);
        SetParams(AtlasTextureRegionYInput!);
        SetParams(AtlasTextureRegionWInput!);
        SetParams(AtlasTextureRegionHInput!);
        SetParams(AtlasTextureMarginXInput!);
        SetParams(AtlasTextureMarginYInput!);
        SetParams(AtlasTextureMarginWInput!);
        SetParams(AtlasTextureMarginHInput!);
        SetParams(NewAtlasTextureMarginXInput!);
        SetParams(NewAtlasTextureMarginYInput!);
        SetParams(NewAtlasTextureMarginWInput!);
        SetParams(NewAtlasTextureMarginHInput!);
        SetParams(CellSizePixelSizeX!);
        SetParams(CellSizePixelSizeY!);
        SetParams(CellSizeOffsetX!);
        SetParams(CellSizeOffsetY!);
        SetParams(CellSizePaddingX!);
        SetParams(CellSizePaddingY!);
        SetParams(CellCountOffsetX!);
        SetParams(CellCountOffsetY!);
        SetParams(CellCountPaddingX!);
        SetParams(CellCountPaddingY!);
        return;

        void SetParams(SpinBox spinBox)
        {
            spinBox.Rounded = rounded;
            spinBox.Step = rounded ? 1 : 0.01f;
        }
    }

    /// <summary>
    ///     Update the metrics displayed in the mini-inspector based on the provided AtlasTextureInfo
    /// </summary>
    /// <param name="atlasTextureInfo"></param>
    private void UpdateInspectingMetrics(EditingAtlasTextureInfo atlasTextureInfo)
    {
        AtlasTextureNameInput!.Text = atlasTextureInfo.Name;
        AtlasTextureNameInput.Editable = atlasTextureInfo.IsTemp;
        if (atlasTextureInfo.IsTemp)
        {
            NewItemLabel!.Show();
            DeleteItemButton!.Show();
        }
        else
        {
            NewItemLabel!.Hide();
            DeleteItemButton!.Hide();
        }

        AtlasTextureRegionXInput!.SetValueNoSignal(atlasTextureInfo.Region.Position.X);
        AtlasTextureRegionYInput!.SetValueNoSignal(atlasTextureInfo.Region.Position.Y);
        AtlasTextureRegionWInput!.SetValueNoSignal(atlasTextureInfo.Region.Size.X);
        AtlasTextureRegionHInput!.SetValueNoSignal(atlasTextureInfo.Region.Size.Y);

        AtlasTextureMarginXInput!.SetValueNoSignal(atlasTextureInfo.Margin.Position.X);
        AtlasTextureMarginYInput!.SetValueNoSignal(atlasTextureInfo.Margin.Position.Y);
        AtlasTextureMarginWInput!.SetValueNoSignal(atlasTextureInfo.Margin.Size.X);
        AtlasTextureMarginHInput!.SetValueNoSignal(atlasTextureInfo.Margin.Size.Y);

        AtlasTextureFilterClipInput!.SetPressedNoSignal(atlasTextureInfo.FilterClip);
    }

    /// <summary>
    ///     Reset the displayed metrics in the mini-inspector
    /// </summary>
    private void ResetInspectingMetrics()
    {
        AtlasTextureNameInput!.Text = string.Empty;

        NewItemLabel!.Hide();
        DeleteItemButton!.Hide();

        AtlasTextureRegionXInput!.SetValueNoSignal(0f);
        AtlasTextureRegionYInput!.SetValueNoSignal(0f);
        AtlasTextureRegionWInput!.SetValueNoSignal(0f);
        AtlasTextureRegionHInput!.SetValueNoSignal(0f);

        AtlasTextureMarginXInput!.SetValueNoSignal(0f);
        AtlasTextureMarginYInput!.SetValueNoSignal(0f);
        AtlasTextureMarginWInput!.SetValueNoSignal(0f);
        AtlasTextureMarginHInput!.SetValueNoSignal(0f);

        AtlasTextureFilterClipInput!.SetPressedNoSignal(false);
    }
}

#endif
