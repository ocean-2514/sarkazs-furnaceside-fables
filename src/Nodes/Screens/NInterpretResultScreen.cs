using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using Skz.Ocean.Entities.Thoughts.Interpreter;
using Skz.Ocean.Nodes.Interpret;

namespace Skz.Ocean.Nodes.Screens;

public partial class NInterpretResultScreen : Control
{
    private InterpretationResult _result;

    private NInterpretResult _resultNode;

    private const string _scenePath = "res://scenes/screens/thought_interpret/interpret_result_screen.tscn";

    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Stop;

        _resultNode = GetNode<NInterpretResult>("InterpretResult");
        _resultNode.Result = _result;
        _resultNode.OnConfirm += CloseOverlay;
    }

    public override void _EnterTree()
    {
        if (_resultNode != null)
        {
            _resultNode.OnConfirm -= CloseOverlay;
        }   
    }

    public static NInterpretResultScreen Create(InterpretationResult result)
    {
        PackedScene? scene = ResourceLoader.Load<PackedScene>(_scenePath);
        if (scene == null)
        {
            Log.Warn("[Sarkaz] failed to load res://scenes/screens/thought_interpret/interpret_result_screen.tscn");
            return null;
        }
        if (scene.Instantiate() is not NInterpretResultScreen screen)
        {
            Log.Warn("[Sarkaz] interpret_result_screen.tscn 根节点没挂 NInterpretResultScreen 脚本");
            return null;
        }
        screen._result = result;
        return screen;
    }

    // 点击面板外的空白处关闭（点面板/按钮会被对应控件吞掉，不会冒泡到这里）
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            CloseOverlay();
            AcceptEvent();
        }
    }

    // ESC：用 _Input + SetInputAsHandled，确保后面列表的 _UnhandledInput 收不到，
    // 否则会把列表也一起关掉。
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            CloseOverlay();
            GetViewport().SetInputAsHandled();
        }
    }

    private void CloseOverlay()
    {
        Visible = false;
        QueueFree();
    }
}