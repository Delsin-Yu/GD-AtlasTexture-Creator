using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Range = Godot.Range;

namespace GodotTextureSlicer;

public partial class UnityAtlasTextureCreator
{
    private static void RegLineEdit(LineEdit control, LineEdit.TextChangedEventHandler call) => control.TextChanged += call;

    private static void RegRangeValueChanged(Range control, Range.ValueChangedEventHandler call) => control.ValueChanged += call;

    private static void RegButtonPressed(BaseButton control, Action call) => control.Pressed += call;

    private static void RegButtonToggled(BaseButton control, BaseButton.ToggledEventHandler call) => control.Toggled += call;

    private static void RegOptionButtonItemSelected(OptionButton control, OptionButton.ItemSelectedEventHandler call) => control.ItemSelected += call;

    private static void RegResourceChanged(Resource resource, Action call) => resource.Changed += call;
}
