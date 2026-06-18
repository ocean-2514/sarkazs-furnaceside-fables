using System;
using Godot;
using Skz.Ocean.Model;

namespace Skz.Ocean.Nodes.Thoughts;

// 思绪“详情条目”：内嵌一个 NThought（图标/星/负荷）+ 右侧标题与描述。
// 挂在 res://scenes/thoughts/thought_display.tscn 根节点(ThoughtDisplay)。
public partial class NThoughtDisplay : Control
{
    private const string _scenePath = "res://scenes/thoughts/thought_display.tscn";

    private ThoughtModel? _model;

    private NThought _thought;       // 内嵌的 thought.tscn 实例
    private RichTextLabel _label;    // 标题 + 描述

    // 被点击时发出。条目本身不决定"点击后做什么"——
    // 由使用它的界面订阅本事件来决定行为
    public event Action<ThoughtModel>? Pressed;

    // 区分"点击"与"拖动滚动"：按下到松开移动超过这个像素就算拖动，不触发点击
    private const float _clickThreshold = 10f;
    private Vector2 _pressPosition;
    private bool _pressedInside;

    public ThoughtModel? Model
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

    public static NThoughtDisplay Create(ThoughtModel model)
    {
        PackedScene scene = ResourceLoader.Load<PackedScene>(_scenePath);
        NThoughtDisplay node = scene.Instantiate<NThoughtDisplay>();
        node.Model = model;
        return node;
    }

    public override void _Ready()
    {
        _thought = GetNode<NThought>("Thought");
        _label = GetNode<RichTextLabel>("Label");

        // 整个条目可点击：根节点收鼠标事件，子节点都不拦截
        MouseFilter = MouseFilterEnum.Stop;
        IgnoreMouseRecursive(this);

        Reload();
    }

    private static void IgnoreMouseRecursive(Node node)
    {
        foreach (Node child in node.GetChildren())
        {
            if (child is Control c) c.MouseFilter = MouseFilterEnum.Ignore;
            IgnoreMouseRecursive(child);
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is not InputEventMouseButton mb || mb.ButtonIndex != MouseButton.Left)
        {
            return;
        }
        if (mb.Pressed)
        {
            _pressedInside = true;
            _pressPosition = mb.GlobalPosition;
        }
        else if (_pressedInside)
        {
            _pressedInside = false;
            // 移动很小才算点击（否则是拖动滚动，交给 NDragScrollContainer 处理）
            if (_model != null && mb.GlobalPosition.DistanceTo(_pressPosition) < _clickThreshold)
            {
                Pressed?.Invoke(_model);
            }
        }
    }

    private void Reload()
    {
        if (!IsNodeReady() || _model == null)
        {
            return;
        }
        _thought.Model = _model;
        _label.Text = BuildText(_model);
    }

    private static string BuildText(ThoughtModel model)
    {
        string title = model.Title.GetFormattedText();
        string description = model.Description.GetFormattedText();
        return
            $"[font_size=30][color=a3a3a3]{title}\n[/color][/font_size]" +
            $"[font_size=25][color=737373]{description}[/color][/font_size]";
    }
}
