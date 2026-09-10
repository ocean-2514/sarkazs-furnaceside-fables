namespace Skz.Ocean.Nodes.Screens;

using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Localization;
using Skz.Ocean.Entities.Ascension;

public partial class NSarkazAscensionPanel : Control
{
    [Signal]
    public delegate void AscensionLevelChangedEventHandler();

    private NSarkazAscensionArrow _leftArrow;
    private NSarkazAscensionArrow _rightArrow;
    private MegaLabel _levelLabel;
    private RichTextLabel _description;

    public int Ascension { get; private set; }

    public override void _Ready()
    {
        _leftArrow = GetNode<NSarkazAscensionArrow>(
            "HBoxContainer/LeftArrowContainer/LeftArrow");
        _rightArrow = GetNode<NSarkazAscensionArrow>(
            "HBoxContainer/RightArrowContainer/RightArrow");
        _levelLabel = GetNode<MegaLabel>(
            "HBoxContainer/AscensionIconContainer/AscensionIcon/AscensionLevel");
        _description = GetNode<RichTextLabel>(
            "HBoxContainer/AscensionDescription/RichTextLabel");

        _leftArrow.Connect(NSarkazAscensionArrow.SignalName.Pressed,
            Callable.From(Decrement));
        _rightArrow.Connect(NSarkazAscensionArrow.SignalName.Pressed,
            Callable.From(Increment));

        Refresh();
    }

    public void Initialize(int preferredLevel)
    {
        SetAscensionLevel(preferredLevel);
        Show();
    }

    public void SetAscensionLevel(int level)
    {
        int clamped = System.Math.Clamp(level,
            SarkazAscensionState.MinLevel, SarkazAscensionState.MaxLevel);
        bool changed = Ascension != clamped;
        Ascension = clamped;
        Refresh();
        if (changed) EmitSignal(SignalName.AscensionLevelChanged);
    }

    private void Decrement() => SetAscensionLevel(Ascension - 1);

    private void Increment() => SetAscensionLevel(Ascension + 1);

    private void Refresh()
    {
        if (_levelLabel == null || _description == null) return;

        _levelLabel.SetTextAutoSize(Ascension.ToString());
        string key = $"LEVEL_{Ascension:D2}";
        string title = new LocString("sarkaz_ascension", $"{key}.title").GetFormattedText();
        string description = new LocString("sarkaz_ascension", $"{key}.description").GetFormattedText();
        _description.Text = $"[b][color=gold]{title}[/color][/b]\n{description}";

        _leftArrow.Visible = Ascension > SarkazAscensionState.MinLevel;
        _rightArrow.Visible = Ascension < SarkazAscensionState.MaxLevel;
    }
}
