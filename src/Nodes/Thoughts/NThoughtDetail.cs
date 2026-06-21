using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Model;

namespace Skz.Ocean.Nodes.Thoughts;

public partial class NThoughtDetail : Control
{
    private const string _scenePath = "res://scenes/thoughts/thought_detail.tscn";

    // 关闭时发出，供打开它的界面做后续处理
    [Signal]
    public delegate void ClosedEventHandler();

    private NThought _thought;
    private Label _title;
    private Label _type;
    private Label _description;
    private RichTextLabel _flavor;
    private TextureRect _useButton;
    private TextureRect _discardButton;

    private ThoughtModel _model;
    public ThoughtModel Model
    {
        get => _model;
        set
        {
            if (_model != value)
            {
                _model = value;
                Reload();
            }
        }
    }

    private Player Player => _model?.Owner; // 思绪是玩家持有的可变实例，Owner 即持有者

    public static NThoughtDetail Create(ThoughtModel model)
    {
        PackedScene scene = ResourceLoader.Load<PackedScene>(_scenePath);
        NThoughtDetail node = scene.Instantiate<NThoughtDetail>();
        node.Model = model;
        return node;
    }

    public override void _Ready()
    {
        _thought = GetNode<NThought>("Thought");
        _title = GetNode<Label>("Title");
        _type = GetNode<Label>("Type");
        _description = GetNode<Label>("Description");
        _flavor = GetNode<RichTextLabel>("Flavor");
        _useButton = GetNode<TextureRect>("UseButton");
        _discardButton = GetNode<TextureRect>("DiscardButton");

        // TextureRect 默认 mouse_filter=Stop，可直接收 gui_input
        _useButton.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(OnUseInput));
        _discardButton.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(OnDiscardInput));

        Reload();
    }

    private void Reload()
    {
        if (!IsNodeReady() || _model == null)
        {
            return;
        }
        _thought.Model = _model;
        _title.Text = _model.Title.GetFormattedText();
        _description.Text = _model.DynamicDescription.GetFormattedText();
        _flavor.Text = "[i][font_size=20][color=6c6868]  “" +
            _model.Flavor.GetFormattedText() + "”[/color][/font_size][/i]";

        switch (_model.ThoughtType)
        {
            case ThoughtType.Inspirer:
                _type.Text = "  灵感";
                break;
            case ThoughtType.Linger:
                _type.Text = "  遗愿";
                break;
            case ThoughtType.Plan:
                _type.Text = "  构想";
                break;
        }

        _useButton.Visible = _model.CanBeUsed();
        _discardButton.Visible = _model.CanBeDiscarded();
    }

    private static bool IsLeftClick(InputEvent @event)
    {
        return @event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left };
    }

    private void OnUseInput(InputEvent @event)
    {
        if (IsLeftClick(@event))
        {
            _ = UseAndClose();
        }
    }

    private void OnDiscardInput(InputEvent @event)
    {
        if (IsLeftClick(@event))
        {
            _ = DiscardAndClose();
        }
    }

    private async Task UseAndClose()
    {
        if (Player != null && _model.CanBeUsed())
        {
            await Player.UseThought(_model);
        }
        Close();
    }

    private async Task DiscardAndClose()
    {
        if (_model != null)
        {
            await ThoughtCmd.Remove(_model);
        }
        Close();
    }

    private void Close()
    {
        // 不自己 QueueFree：本节点通常内嵌在 OneThoughtViewScreen(capstone) 里，
        // 由宿主监听 Closed 决定如何收尾（关闭/返回列表）。
        EmitSignal(SignalName.Closed);
    }
}
