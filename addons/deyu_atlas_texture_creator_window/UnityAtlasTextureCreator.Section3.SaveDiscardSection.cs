#if TOOLS


using System.Collections.Generic;
using Godot;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;
// This script contains the exports and api used by the Save & Discard Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    [Export, ExportSubgroup("Save & Discard Section")] private Button DiscardButton { get; set; }

    [Export] private Button SaveAndUpdateButton { get; set; }

    /// <summary>
    ///     Initialize the Save & Discard button callbacks
    /// </summary>
    private void InitializeSaveDiscardSection()
    {
        RegButtonPressed(
            DiscardButton,
            () =>
            {
                var deletingAtlasTexture = new List<EditingAtlasTextureInfo>();

                foreach (var editingAtlasTextureInfo in m_EditingAtlasTexture)
                {
                    if (editingAtlasTextureInfo.IsTemp) deletingAtlasTexture.Add(editingAtlasTextureInfo);
                    else editingAtlasTextureInfo.DiscardChanges();
                }

                foreach (var editingAtlasTextureInfo in deletingAtlasTexture)
                {
                    if (m_InspectingAtlasTextureInfo == editingAtlasTextureInfo) m_InspectingAtlasTextureInfo = null;

                    m_EditingAtlasTexture.Remove(editingAtlasTextureInfo);
                }

                UpdateControls();
                if (m_InspectingAtlasTextureInfo is not null) UpdateInspectingMetrics(m_InspectingAtlasTextureInfo);
                else ResetInspectingMetrics();
            }
        );
        RegButtonPressed(
            SaveAndUpdateButton,
            () =>
            {
                foreach (var editingAtlasTextureInfo in m_EditingAtlasTexture)
                {
                    var path = editingAtlasTextureInfo.ApplyChanges(m_InspectingTex, m_CurrentSourceTexturePath);
                    if (path is not null) m_EditorFileSystem.UpdateFile(path);
                }

                m_EditorFileSystem.Scan();


                UpdateControls();
                if (m_InspectingAtlasTextureInfo is not null) UpdateInspectingMetrics(m_InspectingAtlasTextureInfo);
                else ResetInspectingMetrics();
            }
        );
    }
}

#endif
