#if TOOLS


using Godot;

namespace GodotTextureSlicer;

/// <summary>
///     Bootstrap script for <see cref="UnityAtlasTextureCreator" />
/// </summary>
[Tool]
public partial class UnityAtlasTextureCreatorBoot : EditorPlugin
{
    private UnityAtlasTextureCreator? _editorWindow;

    /// <summary>
    ///     This window is capable for editing Texture2Ds, except AtlasTexture itself
    /// </summary>
    public override bool _Handles(GodotObject godotObject)
    {
        if (godotObject is not Texture2D texture2D || texture2D is AtlasTexture) return false;

        var path = texture2D.ResourcePath;
        return !path.Contains("::");
    }

    /// <summary>
    ///     This triggers when user double click a supported assets in scene window.
    /// </summary>
    public override void _Edit(GodotObject godotObject)
    {
        if (godotObject is Texture2D texture2D) _editorWindow!.UpdateEditingTexture(texture2D);
        else _editorWindow!.UpdateEditingTexture(null);
    }

    /// <summary>
    ///     Creates the actual window and add it to editor dock.
    /// </summary>
    public override void _EnterTree()
    {
        var scene = GD.Load<PackedScene>("res://addons/deyu_atlas_texture_creator_window/atlas_texture_creator_window.tscn");
        _editorWindow = scene.Instantiate<UnityAtlasTextureCreator>();

        _editorWindow.Initialize(this);

        AddControlToDock(DockSlot.RightBl, _editorWindow);
    }

    /// <summary>
    ///     Delete the corresponding window from the editor dock
    /// </summary>
    public override void _ExitTree()
    {
        RemoveControlFromDocks(_editorWindow);
        _editorWindow!.QueueFree();
    }
}
#endif
