using System;
using System.Diagnostics.CodeAnalysis;
using Godot;
using Range = Godot.Range;

namespace DEYU.GDUtilities.UnityAtlasTextureCreatorUtility;

public partial class UnityAtlasTextureCreator
{
    private static void RegLineEdit([NotNull] LineEdit control, LineEdit.TextChangedEventHandler call) => control.TextChanged += call;

    private static void RegRangeValueChanged([NotNull] Range control, Range.ValueChangedEventHandler call) => control.ValueChanged += call;

    private static void RegButtonPressed([NotNull] BaseButton control, Action call) => control.Pressed += call;

    private static void RegButtonToggled([NotNull] BaseButton control, BaseButton.ToggledEventHandler call) => control.Toggled += call;

    private static void RegOptionButtonItemSelected([NotNull] OptionButton control, OptionButton.ItemSelectedEventHandler call) => control.ItemSelected += call;

    private static void RegResourceChanged([NotNull] Resource resource, Action call) => resource.Changed += call;
}
