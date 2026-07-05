using System;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Entities.Thoughts.Interpreter;
using Skz.Ocean.Model;

namespace Skz.Ocean.Nodes.Interpret;

public partial class NInterpretResult : Control
{
    private TextureRect _background;
    private TextureRect _icon;
    private Label _title;
    private MegaRichTextLabel _description;
    private Control _confirmPanel;

    private const string _scenePath = "res://scenes/screens/thought_interpret/interpret_result.tscn";
    
    private InterpretationResult _result;

    public string RelicBackgroundPath => ImageHelper.GetImagePath("ui/relic_result_background.png");
    public string ThoughtBackgroundPath => ImageHelper.GetImagePath("ui/thought_result_background.png");
    public string ShieldBackgroundPath => ImageHelper.GetImagePath("ui/shield_result_background.png");

    public Texture2D RelicBackground => ResourceLoader.Load<Texture2D>(RelicBackgroundPath);
    public Texture2D ThoughtBackground => ResourceLoader.Load<Texture2D>(ThoughtBackgroundPath);
    public Texture2D ShieldBackground => ResourceLoader.Load<Texture2D>(ShieldBackgroundPath);

    public event Action? OnConfirm;

    public InterpretationResult Result
    {
        get => _result;
        set
        {
            if (_result != value)
            {
                _result = value;
                Populate();
            }
        }
    }

    public override void _Ready()
    {
        _background = GetNode<TextureRect>("Background");
        _icon = GetNode<TextureRect>("Icon");
        _title = GetNode<Label>("Title");
        _description = GetNode<MegaRichTextLabel>("Description");
        _confirmPanel = GetNode<Control>("ConfirmPanel");
        _confirmPanel.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(OnConfirmInput));

        // Result 可能在进树前就被赋值（Create 路径），此时 setter 里的 Populate 被守卫跳过，这里补跑。
        Populate();
    }

    public static NInterpretResult? Create(InterpretationResult result)
    {
        PackedScene? scene = ResourceLoader.Load<PackedScene>(_scenePath);
        if (scene == null)
        {
            Log.Warn("[Sarkaz] failed to load " + _scenePath);
            return null;
        }
        if (scene.Instantiate() is not NInterpretResult interpretResult)
        {
            Log.Warn("[Sarkaz] interpret_result.tscn 根节点没挂 NInterpretResult 脚本");
            return null;
        }
        interpretResult.Result = result;
        
        return interpretResult;
    }

    private void Populate()
    {
        // _Ready 之前（节点引用未取到）或还没有结果时跳过；_Ready 里会补跑一次。
        if (_background == null || _result == null)
        {
            return;
        }
        switch (_result.ResultType)
        {
            case InterpretationResultType.RELIC:
                RelicModel? relic = _result.RelicResult;
                _background.Texture = RelicBackground;
                _icon.Texture = relic?.Icon;
                _title.Text = relic?.Title.GetFormattedText();
                _description.Text = ProcessDescription(relic?.DynamicDescription.GetFormattedText() ?? string.Empty);
                break;
            case InterpretationResultType.THOUGHT:
                ThoughtModel? thought = _result.ThoughtResult;
                _background.Texture = ThoughtBackground;
                _icon.Texture = thought?.Icon;
                _title.Text = thought?.Title.GetFormattedText();
                _description.Text = ProcessDescription(thought?.DynamicDescription.GetFormattedText() ?? string.Empty);
                break;
            case InterpretationResultType.SHIELD:
                int amount = _result.ShieldResult;
                _background.Texture = ShieldBackground;
                _icon.Visible = false;
                _title.Text = "护盾";
                _description.Text = $"[font_size=50][color=5a95c4]+[/color][color=fafafa]{amount}[/color][/font_size]";
                break;
        }
    }

    private void OnConfirmInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            OnConfirm?.Invoke();
            AcceptEvent();
        }
    }

    private string ProcessDescription(string text)
    {
        text = "[font_size=25]" + text + "[/font_size]";
        return text;
    }
}