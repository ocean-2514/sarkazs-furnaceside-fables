using System.Collections.Generic;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using MegaCrit.Sts2.Core.Nodes.Screens.ScreenContext;
using MegaCrit.Sts2.Core.Random;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts.Interpreter;
using Skz.Ocean.Model;
using Skz.Ocean.Nodes.Thoughts;

namespace Skz.Ocean.Nodes.Screens;

public partial class NThoughtInterpretScreen : Control, ICapstoneScreen, IScreenContext
{
    private Player _player;

    private const string _scenePath = "res://scenes/screens/thought_interpret/thought_interpret_screen.tscn";

    public string InterpretIconPath1 => ImageHelper.GetImagePath("ui/interpret_1.png");
    public string InterpretIconPath2 => ImageHelper.GetImagePath("ui/interpret_2.png");

    public Texture2D InterpretIcon1 => ResourceLoader.Load<Texture2D>(InterpretIconPath1);
    public Texture2D InterpretIcon2 => ResourceLoader.Load<Texture2D>(InterpretIconPath2);

    private GridContainer _inspirerContainer;
    private GridContainer _lingerContainer;
    private GridContainer _planContainer;
    private Control _inspirerLabel;
    private Control _lingerLabel;
    private Control _planLabel;
    private TextureRect _interpretButton;
    private NThoughtDisplay _thoughtDisplay1;
    private NThoughtDisplay _thoughtDisplay2;

    private ThoughtModel _model1 = null;
    private ThoughtModel _model2 = null;

    public Control? DefaultFocusedControl => null;
    public bool UseSharedBackstop => true;

    public NetScreenType ScreenType => NetScreenType.None;

    public static NThoughtInterpretScreen? ShowScreen(Player player)
    {
        PackedScene? scene = ResourceLoader.Load<PackedScene>(_scenePath);
        if (scene == null)
        {
            Log.Warn("[Sarkaz] failed to load " + _scenePath);
            return null;
        }
        if (scene.Instantiate() is not NThoughtInterpretScreen screen)
        {
            Log.Warn("[Sarkaz] thought_interpret_screen.tscn 根节点没挂 NThoughtInterpretScreen 脚本");
            return null;
        }
        NCapstoneContainer? container = NCapstoneContainer.Instance;
        if (container == null)
        {
            Log.Warn("[Sarkaz] NCapstoneContainer 不可用（不在 run 中？）");
            screen.QueueFree();
            return null;
        }
        screen._player = player; 
        container.Open(screen);
        return screen;
    }

    public override void _Ready()
    {
        _inspirerContainer = GetNode<GridContainer>("%InspirerContainer");
        _lingerContainer = GetNode<GridContainer>("%LingerContainer");
        _planContainer = GetNode<GridContainer>("%PlanContainer");
        _inspirerLabel = GetNode<Control>("%LabelContainer1"); // 灵感
        _lingerLabel = GetNode<Control>("%LabelContainer2");   // 遗愿
        _planLabel = GetNode<Control>("%LabelContainer3");     // 构想
        _interpretButton = GetNode<TextureRect>("%InterpretButton");
        _thoughtDisplay1 = GetNode<NThoughtDisplay>("%Thought1");
        _thoughtDisplay2 = GetNode<NThoughtDisplay>("%Thought2");

        _interpretButton.GuiInput += OnInterpretButtonGuiInput;

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

    private void Populate()
    {
        _model1 = null;
        _model2 = null;
        _thoughtDisplay1.Visible = false;
        _thoughtDisplay2.Visible = false;
        UpdateInterpretButtonState();
        PopulateRightPanel();
    }

    private void PopulateRightPanel()
    {
        PopulateSection(_lingerContainer, _lingerLabel, _player.GetLingers());
        PopulateSection(_inspirerContainer, _inspirerLabel, _player.GetInspirers());
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
            NThought nThought = NThought.Create(thought);
            nThought.Pressed += OnThoughtPressed;
            container.AddChild(nThought);
        }
    }

    private void OnThoughtPressed(NThought thought)
    {
        ThoughtModel model = thought.Model;
        if (_model1 == null && model != _model2)
        {
            _model1 = model;
            _thoughtDisplay1.Visible = true;
            _thoughtDisplay1.Model = model;
            thought.SetOverlayVisible(true);
        }
        else if (_model2 == null && model != _model1)
        {
            _model2 = model;
            _thoughtDisplay2.Visible = true;
            _thoughtDisplay2.Model = model;
            thought.SetOverlayVisible(true);
        }
        else if (model == _model1)
        {
            _model1 = null;
            _thoughtDisplay1.Visible = false;
            _thoughtDisplay1.Model = null;
            thought.SetOverlayVisible(false);
        }
        else if (model == _model2)
        {
            _model2 = null;
            _thoughtDisplay2.Visible = false;
            _thoughtDisplay2.Model = null;
            thought.SetOverlayVisible(false);
        }

        UpdateInterpretButtonState();
    }

    private void UpdateInterpretButtonState()
    {
        if (_model1 != null && _model2 != null)
        {
            _interpretButton.Texture = InterpretIcon2;
            _interpretButton.MouseFilter = MouseFilterEnum.Stop;
        }
        else
        {
            _interpretButton.Texture = InterpretIcon1;
            _interpretButton.MouseFilter = MouseFilterEnum.Ignore;
        }
    }

    private void OnInterpretButtonGuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left }) return;
        if (_model1 == null || _model2 == null) return;
        DoInterpret();
    }

    private async void DoInterpret()
    {
        // 先把选中的两张存下来：消费时触发 PlayerThoughtChanged → Populate 会把 _model1/_model2 清空。
        ThoughtModel model1 = _model1;
        ThoughtModel model2 = _model2;

        Rng rng = _player.PlayerRng.Rewards;
        InterpretationResult result = Interpreter.Interpret(model1, model2, rng);
        Log.Info($"[Sarkaz] 解读结果: {result.ResultType}, thought={result.ThoughtResult?.Id}, relic={result.RelicResult?.Id}, shield={result.ShieldResult}");

        if (result.ResultType == InterpretationResultType.NONE)
        {
            // 解读失败（错误分支），不消耗思绪。
            return;
        }

        // 消费两张输入思绪（会触发列表刷新、清空选中）
        await ThoughtCmd.Remove(model1);
        await ThoughtCmd.Remove(model2);

        // 发放结果
        switch (result.ResultType)
        {
            case InterpretationResultType.THOUGHT:
                await ThoughtCmd.Obtain(result.ThoughtResult, _player);
                break;
            case InterpretationResultType.RELIC:
                await RelicCmd.Obtain(result.RelicResult, _player);
                break;
            case InterpretationResultType.SHIELD:
                _player.GainShield(result.ShieldResult);
                break;
        }

        // 解读完成钩子（思维捕手等遗物响应）
        await Hooks.SarkazHook.AfterInterpreted(_player.RunState, _player, result);

        ShowResultOverlay(result);
    }

    // 结果展示 overlay：盖在本界面之上，点空白/ESC 只关它自己。
    private void ShowResultOverlay(InterpretationResult result)
    {
        NInterpretResultScreen overlay = NInterpretResultScreen.Create(result);
        if (overlay == null)
        {
            return;
        }
        AddChild(overlay);
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

    public void AfterCapstoneOpened()
    {
    }

    public void AfterCapstoneClosed()
    {
        Visible = false;
        QueueFree();
    }
}