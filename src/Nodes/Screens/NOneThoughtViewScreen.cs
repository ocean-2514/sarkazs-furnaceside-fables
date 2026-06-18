using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using Skz.Ocean.Model;
using Skz.Ocean.Nodes.Thoughts;

namespace Skz.Ocean.Nodes.Screens;

public partial class NOneThoughtViewScreen : Control, ICapstoneScreen, IScreenContext
{
    private const string _scenePath = "res://scenes/screens/thought_select/one_thought_view_screen.tscn";

    private ThoughtModel _thought = null!;
    private NThoughtDetail _detail;

    public ThoughtModel Model => _thought;
    public NetScreenType ScreenType => NetScreenType.None;
    public bool UseSharedBackstop => true;

    public Control? DefaultFocusedControl => null;

    public static NOneThoughtViewScreen Create(ThoughtModel thought)
    {
        PackedScene scene = ResourceLoader.Load<PackedScene>(_scenePath);
        NOneThoughtViewScreen node = scene.Instantiate<NOneThoughtViewScreen>();
        node._thought = thought;
        return node;
    }

    public static NOneThoughtViewScreen? ShowScreen(ThoughtModel thought)
    {
        PackedScene? scene = ResourceLoader.Load<PackedScene>(_scenePath);
        if (scene == null)
        {
            Log.Warn("[Sarkaz] failed to load " + _scenePath);
            return null;
        }
        if (scene.Instantiate() is not NOneThoughtViewScreen screen)
        {
            Log.Warn("[Sarkaz] thought_view_screen.tscn 根节点没挂 NOneThoughtViewScreen 脚本");
            return null;
        }
        NCapstoneContainer? container = NCapstoneContainer.Instance;
        if (container == null)
        {
            Log.Warn("[Sarkaz] NCapstoneContainer 不可用（不在 run 中？）");
            screen.QueueFree();
            return null;
        }
        screen._thought = thought; // 必须在 Open(=AddChild=_Ready) 之前
        container.Open(screen);
        return screen;
    }

    public override void _Ready()
    {
        // 全屏 Stop：挡住鼠标点击，避免穿透到后面的列表
        MouseFilter = MouseFilterEnum.Stop;

        _detail = GetNode<NThoughtDetail>("ThoughtDetail");
        _detail.Model = _thought;
        _detail.Closed += CloseOverlay; // use/discard 后详情发 Closed → 关闭覆盖层
    }

    public override void _ExitTree()
    {
        if (_detail != null)
        {
            _detail.Closed -= CloseOverlay;
        }
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
        if (NCapstoneContainer.Instance.CurrentCapstoneScreen is NOneThoughtViewScreen)
        {
            NCapstoneContainer.Instance.Close();
        } 
        else
        {
            QueueFree();
        }
    }

    public void AfterCapstoneOpened()
    {
    }

    public void AfterCapstoneClosed()
    {
        Visible = false;
        QueueFree();
    }
}
