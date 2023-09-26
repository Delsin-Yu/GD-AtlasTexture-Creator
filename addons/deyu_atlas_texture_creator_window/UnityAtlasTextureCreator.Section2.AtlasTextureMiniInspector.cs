#if TOOLS

#region

using Godot;

#endregion

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

// This script contains the exports and api used by the AtlasTexture Mini Inspector Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    [Export, ExportSubgroup("AtlasTexture Mini Inspector Section")] private Control MiniInspectorWindow { get; set; }
    [Export] private Label NewItemLabel { get; set; }
    [Export] private Button DeleteItemButton { get; set; }
    [Export, ExportSubgroup("AtlasTexture Mini Inspector Section/Inputs")] private LineEdit AtlasTextureNameInput { get; set; }
    [Export, ExportSubgroup("AtlasTexture Mini Inspector Section/Inputs/Region")] private SpinBox AtlasTextureRegionXInput { get; set; }
    [Export] private SpinBox AtlasTextureRegionYInput { get; set; }
    [Export] private SpinBox AtlasTextureRegionWInput { get; set; }
    [Export] private SpinBox AtlasTextureRegionHInput { get; set; }
    [Export, ExportSubgroup("AtlasTexture Mini Inspector Section/Inputs/Margin")] private SpinBox AtlasTextureMarginXInput { get; set; }
    [Export] private SpinBox AtlasTextureMarginYInput { get; set; }
    [Export] private SpinBox AtlasTextureMarginWInput { get; set; }
    [Export] private SpinBox AtlasTextureMarginHInput { get; set; }
    [Export] private CheckBox AtlasTextureFilterClipInput { get; set; }

    /// <summary>
    /// Initialize callbacks for the MiniInspector
    /// </summary>
    private void InitializeAtlasTextureMiniInspector()
    {
        AtlasTextureNameInput.TextChanged +=
            newText =>
            {
                if (m_InspectingAtlasTextureInfo is null || !m_InspectingAtlasTextureInfo.IsTemp) return;
                if (m_InspectingAtlasTextureInfo.TrySetName(newText)) UpdateControls();
            };

        AtlasTextureRegionXInput.ValueChanged +=
            newRegionX =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                var value = m_InspectingAtlasTextureInfo.Region;
                value.Position = new((float)newRegionX, value.Position.Y);
                if (m_InspectingAtlasTextureInfo.TrySetRegion(value)) UpdateControls();
            };
        AtlasTextureRegionYInput.ValueChanged +=
            newRegionY =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                var value = m_InspectingAtlasTextureInfo.Region;
                value.Position = new(value.Position.X, (float)newRegionY);
                if (m_InspectingAtlasTextureInfo.TrySetRegion(value)) UpdateControls();
            };
        AtlasTextureRegionWInput.ValueChanged +=
            newRegionW =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                var value = m_InspectingAtlasTextureInfo.Region;
                value.Size = new((float)newRegionW, value.Size.Y);
                if (m_InspectingAtlasTextureInfo.TrySetRegion(value)) UpdateControls();
            };
        AtlasTextureRegionHInput.ValueChanged +=
            newRegionH =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                var value = m_InspectingAtlasTextureInfo.Region;
                value.Size = new(value.Size.X, (float)newRegionH);
                if (m_InspectingAtlasTextureInfo.TrySetRegion(value)) UpdateControls();
            };

        AtlasTextureMarginXInput.ValueChanged +=
            newMarginX =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                var value = m_InspectingAtlasTextureInfo.Margin;
                value.Position = new((float)newMarginX, value.Position.Y);
                if (m_InspectingAtlasTextureInfo.TrySetMargin(value)) UpdateControls();
            };
        AtlasTextureMarginYInput.ValueChanged +=
            newMarginY =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                var value = m_InspectingAtlasTextureInfo.Margin;
                value.Position = new(value.Position.X, (float)newMarginY);
                if (m_InspectingAtlasTextureInfo.TrySetMargin(value)) UpdateControls();
            };
        AtlasTextureMarginWInput.ValueChanged +=
            newMarginW =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                var value = m_InspectingAtlasTextureInfo.Margin;
                value.Size = new((float)newMarginW, value.Size.Y);
                if (m_InspectingAtlasTextureInfo.TrySetMargin(value)) UpdateControls();
            };
        AtlasTextureMarginHInput.ValueChanged +=
            newMarginH =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                var value = m_InspectingAtlasTextureInfo.Margin;
                value.Size = new(value.Size.X, (float)newMarginH);
                if (m_InspectingAtlasTextureInfo.TrySetMargin(value)) UpdateControls();
            };
        AtlasTextureFilterClipInput.Toggled +=
            newFilterClip =>
            {
                if (m_InspectingAtlasTextureInfo is null) return;
                if (m_InspectingAtlasTextureInfo.TrySetFilterClip(newFilterClip)) UpdateControls();
            };
        DeleteItemButton.Pressed +=
            () =>
            {
                if (m_InspectingAtlasTextureInfo is null || !m_InspectingAtlasTextureInfo.IsTemp) return;
                m_EditingAtlasTexture.Remove(m_InspectingAtlasTextureInfo);
                m_InspectingAtlasTextureInfo = null;
                UpdateControls();
            };
    }

    /// <summary>
    /// Set the spin box mode (rounded or not) for all value input related controls
    /// </summary>
    /// <param name="rounded"></param>
    private void SetSpinBoxMode(bool rounded)
    {
        Set(AtlasTextureRegionXInput);
        Set(AtlasTextureRegionYInput);
        Set(AtlasTextureRegionWInput);
        Set(AtlasTextureRegionHInput);
        Set(AtlasTextureMarginXInput);
        Set(AtlasTextureMarginYInput);
        Set(AtlasTextureMarginWInput);
        Set(AtlasTextureMarginHInput);
        Set(NewAtlasTextureMarginXInput);
        Set(NewAtlasTextureMarginYInput);
        Set(NewAtlasTextureMarginWInput);
        Set(NewAtlasTextureMarginHInput);
        Set(CellSize_PixelSizeX);
        Set(CellSize_PixelSizeY);
        Set(CellSize_OffsetX);
        Set(CellSize_OffsetY);
        Set(CellSize_PaddingX);
        Set(CellSize_PaddingY);
        Set(CellCount_OffsetX);
        Set(CellCount_OffsetY);
        Set(CellCount_PaddingX);
        Set(CellCount_PaddingY);
        return;

        void Set(SpinBox spinBox)
        {
            if(spinBox is null) return;
            spinBox.Rounded = rounded;
        }
    }

    /// <summary>
    /// Update the metrics displayed in the mini-inspector based on the provided AtlasTextureInfo
    /// </summary>
    /// <param name="atlasTextureInfo"></param>
    private void UpdateInspectingMetrics(EditingAtlasTextureInfo atlasTextureInfo)
    {
        AtlasTextureNameInput.Text = atlasTextureInfo.Name;
        AtlasTextureNameInput.Editable = atlasTextureInfo.IsTemp;
        if (atlasTextureInfo.IsTemp)
        {
            NewItemLabel.Show();
            DeleteItemButton.Show();
        }
        else
        {
            NewItemLabel.Hide();
            DeleteItemButton.Hide();
        }

        AtlasTextureRegionXInput.SetValueNoSignal(atlasTextureInfo.Region.Position.X);
        AtlasTextureRegionYInput.SetValueNoSignal(atlasTextureInfo.Region.Position.Y);
        AtlasTextureRegionWInput.SetValueNoSignal(atlasTextureInfo.Region.Size.X);
        AtlasTextureRegionHInput.SetValueNoSignal(atlasTextureInfo.Region.Size.Y);

        AtlasTextureMarginXInput.SetValueNoSignal(atlasTextureInfo.Margin.Position.X);
        AtlasTextureMarginYInput.SetValueNoSignal(atlasTextureInfo.Margin.Position.Y);
        AtlasTextureMarginWInput.SetValueNoSignal(atlasTextureInfo.Margin.Size.X);
        AtlasTextureMarginHInput.SetValueNoSignal(atlasTextureInfo.Margin.Size.Y);

        AtlasTextureFilterClipInput.SetPressedNoSignal(atlasTextureInfo.FilterClip);
    }

    /// <summary>
    /// Reset the displayed metrics in the mini-inspector
    /// </summary>
    private void ResetInspectingMetrics()
    {
        AtlasTextureNameInput.Text = string.Empty;

        AtlasTextureRegionXInput.SetValueNoSignal(0f);
        AtlasTextureRegionYInput.SetValueNoSignal(0f);
        AtlasTextureRegionWInput.SetValueNoSignal(0f);
        AtlasTextureRegionHInput.SetValueNoSignal(0f);

        AtlasTextureMarginXInput.SetValueNoSignal(0f);
        AtlasTextureMarginYInput.SetValueNoSignal(0f);
        AtlasTextureMarginWInput.SetValueNoSignal(0f);
        AtlasTextureMarginHInput.SetValueNoSignal(0f);

        AtlasTextureFilterClipInput.SetPressedNoSignal(false);
    }
}

#endif