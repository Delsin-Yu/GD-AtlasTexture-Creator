#if TOOLS


using System.Collections.Generic;
using Godot;

namespace GodotTextureSlicer;
// This script contains the exports and api used by the Save & Discard Section of the UnityAtlasTextureCreator

public partial class UnityAtlasTextureCreator
{
    [Export, ExportSubgroup("Save & Discard Section")] private Button? DiscardButton { get; set; }

    [Export] private Button? SaveAndUpdateButton { get; set; }

    /// <summary>
    ///     Initialize the Save Discard button callbacks
    /// </summary>
    private void InitializeSaveDiscardSection()
    {
        RegButtonPressed(
            DiscardButton!,
            () =>
            {
                var deletingAtlasTexture = new List<EditingAtlasTextureInfo>();

                foreach (var editingAtlasTextureInfo in _editingAtlasTexture)
                {
                    if (editingAtlasTextureInfo.IsTemp) deletingAtlasTexture.Add(editingAtlasTextureInfo);
                    else editingAtlasTextureInfo.DiscardChanges();
                }

                foreach (var editingAtlasTextureInfo in deletingAtlasTexture)
                {
                    if (_inspectingAtlasTextureInfo == editingAtlasTextureInfo) _inspectingAtlasTextureInfo = null;

                    _editingAtlasTexture.Remove(editingAtlasTextureInfo);
                }

                UpdateControls();
                if (_inspectingAtlasTextureInfo is not null) UpdateInspectingMetrics(_inspectingAtlasTextureInfo);
                else ResetInspectingMetrics();
            }
        );
        RegButtonPressed(
            SaveAndUpdateButton!,
            () =>
            {
                foreach (var editingAtlasTextureInfo in _editingAtlasTexture)
                {
                    var path = editingAtlasTextureInfo.ApplyChanges(_inspectingTex!, _currentSourceTexturePath!);
                    _editorFileSystem!.UpdateFile(path);
                }

                _editorFileSystem!.Scan();


                UpdateControls();
                if (_inspectingAtlasTextureInfo is not null) UpdateInspectingMetrics(_inspectingAtlasTextureInfo);
                else ResetInspectingMetrics();
            }
        );
    }
}

#endif
