using System;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Nodes.Screens;

namespace Skz.Ocean.Nodes.TopBar;

public partial class NTopBarThoughtButton : Control, ITopBarScene
{
    private Player? _player;
    private ColorRect? _bar;
    private ColorRect? _background;
    private TextureRect? _burdenIcon;
    private MegaRichTextLabel? _textLabel;

    private const string LucidBarColor = "#185e48";
    private const string LucidTextColor = "#00ae76";
    private const string FracturedBarColor = "#644d26";
    private const string FracturedTextColor = "#d69428";
    private const string ObstructedBarColor = "#5e1f1f";
    private const string ObstructedTextColor = "#cf302c";

    public Texture2D LucidTexture = ResourceLoader.Load<Texture2D>("res://images/ui/mind_lucid.png");
    public Texture2D FracturedTexture = ResourceLoader.Load<Texture2D>("res://images/ui/mind_fractured.png");
    public Texture2D ObstructedTexture = ResourceLoader.Load<Texture2D>("res://images/ui/mind_obstructed.png");
    public Font Font = ResourceLoader.Load<Font>("res://themes/sarkaz.tres");
    public override void _Ready()
    {
        _bar = GetNodeOrNull<ColorRect>("MarginContainer/HBoxContainer/RightPanel/Bar");
        _background = GetNodeOrNull<ColorRect>("MarginContainer/HBoxContainer/RightPanel/BackGround");
        _burdenIcon = GetNodeOrNull<TextureRect>("MarginContainer/HBoxContainer/Control/BurdenIcon");
        _textLabel = GetNodeOrNull<MegaRichTextLabel>("MarginContainer/HBoxContainer/RightPanel/TextLabel");
        _textLabel.AddThemeFontOverride("normal_font", Font);

        // 让整个区域可点击：根节点接收鼠标事件，所有子节点不拦截
        MouseFilter = MouseFilterEnum.Stop;
        IgnoreMouseRecursive(this);
        Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(OnGuiInput));
    }
    
    private static void IgnoreMouseRecursive(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is Control c) c.MouseFilter = MouseFilterEnum.Ignore;
            IgnoreMouseRecursive(child);
        }
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            ToggleScreen();
        }
    }

    private void ToggleScreen()
    {
        if (_player == null)
        {
            return;
        }
        NCapstoneContainer? container = NCapstoneContainer.Instance;
        if (container == null)
        {
            return;
        }
        if (container.CurrentCapstoneScreen is NThoughtViewScreen)
        {
            container.Close();
        }
        else
        {
            NThoughtViewScreen.ShowScreen(_player);
        }
    }

    public void Initialize(Player player)
    {
        Log.Info("NTopBarThoughtButton Initializer called");
        _player = player;
        _player.GetExtraData().ToilLimitChanged += Refresh;
        _player.GetExtraData().PlayerThoughtChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        if (_player != null)
        {
            _player.GetExtraData().ToilLimitChanged -= Refresh;
            _player.GetExtraData().PlayerThoughtChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        Log.Info("NTopBarThoughtButton Refresh called");
        if (_textLabel == null || _player == null)
        {
            Log.Warn($"_textLabel: {_textLabel}, _player: {_player}");
            return;
        }
        
        int lucidLimit = _player.GetLucidLimit();
        int totalToil = _player.GetTotalToil();
        MindState mindState = _player.GetMindState();
        string barColor = "", textColor = "";
        Texture2D texture = null;
        switch (mindState)
        {
            case MindState.Lucid:
                barColor = LucidBarColor;
                textColor = LucidTextColor;
                texture = LucidTexture;
                break;
            case MindState.Fractured:
                barColor = FracturedBarColor;
                textColor = FracturedTextColor;
                texture = FracturedTexture;
                break;
            case MindState.Obstructed:
                barColor = ObstructedBarColor;
                textColor = ObstructedTextColor;
                texture = ObstructedTexture;
                break;
        }
        _burdenIcon.Texture = texture;
        _bar.Color = Color.FromString(barColor, Colors.White);
        float ratio = lucidLimit > 0 ? Math.Min(1f, (float)totalToil / lucidLimit) : 1f;
        _bar.Size = new Vector2(_background.Size.X * ratio, _bar.Size.Y);
        _textLabel.Text =
            $"[color={textColor}][font_size=36]{totalToil}[/font_size][/color]" +
            $"[color=#bababa][font_size=30] / {lucidLimit}[/font_size][/color]";
        
        Log.Debug($"barColor: {barColor}, textColor: {textColor}, lucidLimit:{lucidLimit}, totalToil: {totalToil}");
    }
}