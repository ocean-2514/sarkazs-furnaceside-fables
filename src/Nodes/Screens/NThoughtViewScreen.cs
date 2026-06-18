using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Model;
using Skz.Ocean.Nodes.Thoughts;

namespace Skz.Ocean.Nodes.Screens;

// 思绪详情界面（Capstone 弹窗），按类型分三栏展示玩家持有的思绪。
// 挂在 res://scenes/screens/thought_select/thought_view_screen.tscn 根节点。
public partial class NThoughtViewScreen : Control, ICapstoneScreen, IScreenContext
{
    private const string _scenePath = "res://scenes/screens/thought_select/thought_view_screen.tscn";

    private Player _player = null!;

    private ColorRect _backGround;
    private Control _scroll;
    private GridContainer _inspirerContainer;
    private GridContainer _lingerContainer;
    private GridContainer _planContainer;
    private Control _inspirerLabel;
    private Control _lingerLabel;
    private Control _planLabel;
    private NToilIndicator _toilIndicator;

    private NOneThoughtViewScreen _overlay = null;
    public NOneThoughtViewScreen Overlay => _overlay;

    public NetScreenType ScreenType => NetScreenType.None;

    public bool UseSharedBackstop => true;

    public Control? DefaultFocusedControl => null;

    public static NThoughtViewScreen? ShowScreen(Player player)
    {
        PackedScene? scene = ResourceLoader.Load<PackedScene>(_scenePath);
        if (scene == null)
        {
            Log.Warn("[Sarkaz] failed to load " + _scenePath);
            return null;
        }
        if (scene.Instantiate() is not NThoughtViewScreen screen)
        {
            Log.Warn("[Sarkaz] thought_view_screen.tscn 根节点没挂 NThoughtViewScreen 脚本");
            return null;
        }
        NCapstoneContainer? container = NCapstoneContainer.Instance;
        if (container == null)
        {
            Log.Warn("[Sarkaz] NCapstoneContainer 不可用（不在 run 中？）");
            screen.QueueFree();
            return null;
        }
        screen._player = player; // 必须在 Open(=AddChild=_Ready) 之前
        container.Open(screen);
        return screen;
    }

    public override void _Ready()
    {
        _backGround = GetNode<ColorRect>("%BackGround");
        _scroll = GetNodeOrNull<Control>("BackGround/MarginContainer/ScrollContainer");
        _inspirerContainer = GetNode<GridContainer>("%InspirerContainer");
        _lingerContainer = GetNode<GridContainer>("%LingerContainer");
        _planContainer = GetNode<GridContainer>("%PlanContainer");
        _inspirerLabel = GetNode<Control>("%LabelContainer1"); // 灵感
        _lingerLabel = GetNode<Control>("%LabelContainer2");   // 遗愿
        _planLabel = GetNode<Control>("%LabelContainer3");     // 构想
        _toilIndicator = GetNode<NToilIndicator>("%ToilIndicator");
        _toilIndicator.Initialize(_player);

        Populate();
        _player.GetExtraData().PlayerThoughtChanged += Populate;
    }

    public override void _ExitTree()
    {
        if (_player != null)
        {
            _player.GetExtraData().PlayerThoughtChanged -= Populate;
        }
    }

    // 点击空白处关闭：能冒泡到根节点的点击，说明点的不是面板/条目（那些会被对应控件吞掉），即空白处。
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left })
        {
            NCapstoneContainer.Instance?.Close();
            AcceptEvent();
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            NCapstoneContainer.Instance?.Close();
            GetViewport().SetInputAsHandled();
        }
    }

    private void Populate()
    {
        PopulateSection(_inspirerContainer, _inspirerLabel, _player.GetInspirers());
        PopulateSection(_lingerContainer, _lingerLabel, _player.GetLingers());
        PopulateSection(_planContainer, _planLabel, _player.GetPlans());
    }

    private void PopulateSection(GridContainer container, Control label, IReadOnlyList<ThoughtModel> thoughts)
    {
        // 清掉旧条目（含场景里预置的占位 ThoughtDisplay）
        foreach (Node child in container.GetChildren())
        {
            container.RemoveChild(child);
            child.QueueFree();
        }

        // 该类型为空时，连标题带容器一起隐藏
        bool any = thoughts.Count > 0;
        container.Visible = any;
        label.Visible = any;

        foreach (ThoughtModel thought in thoughts)
        {
            NThoughtDisplay display = NThoughtDisplay.Create(thought);
            display.Pressed += OnThoughtPressed;
            container.AddChild(display);
        }
    }

    // 本界面对“点击思绪”的处理：把单思绪详情作为覆盖层叠在列表之上（列表留在原位）。
    private void OnThoughtPressed(ThoughtModel thought)
    {
        ChangeOverlay(thought);
    }

    public void ChangeOverlay(ThoughtModel? thought)
    {
        if (_overlay != null)
        {
            _overlay.Visible = false;
            _overlay.QueueFree();
        }
        if (thought == null) return;

        _overlay = NOneThoughtViewScreen.Create(thought);
        // 预览期间禁用列表的拖动滚动(NDragScrollContainer 用 _Input)，
        // 否则在预览上拖动会滚到后面看不见的列表；覆盖层关闭后恢复。
        _scroll?.SetProcessInput(false);
        _overlay.TreeExited += () =>
        {
            if (GodotObject.IsInstanceValid(_scroll))
            {
                _scroll.SetProcessInput(true);
            }
            _overlay = null;
        };
        AddChild(_overlay);
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
