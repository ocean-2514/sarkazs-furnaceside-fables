using System;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using Skz.Ocean.Model;

namespace Skz.Ocean.Nodes.Thoughts;

// 单个思绪的视觉节点（仿 NCard 的纯渲染职责）。挂在 res://scenes/thoughts/thought.tscn 根节点。
// 不做交互（点击/悬停）——交互在使用它的界面里另行处理。
public partial class NThought : Control
{
    private const string _scenePath = "res://scenes/thoughts/thought.tscn";

    public static readonly Vector2 defaultSize = new Vector2(130f, 130f);

    private ThoughtModel? _model;

    private TextureRect _icon;
    private HBoxContainer _starContainer;
    private TextureRect _star; // 场景里那颗星，作为复制模板
    private Control _toilContainer;
    private Control _overlay;
    private Label _label;

    private bool _pressedInside;
    private Vector2 _pressPosition;
    private const float _clickThreshold = 10f;


    // 一开始就确定：控制星星/负荷容器是否显示
    public bool StarVisible { get; set; } = true;
    public bool ToilVisible { get; set; } = true;
    public bool OverlayVisible { get; set; } = false;

    public event Action<NThought>? Pressed;

    public ThoughtModel? Model
    {
        get
        {
            return _model;
        }
        set
        {
            if (_model != value)
            {
                ThoughtModel model = _model;
                UnsubscribeFromModel(model);
                _model = value;
                Reload();
                SubscribeToModel(_model);
            }
        }
    }

    // 实例化一个思绪节点；可见性在进树前定好。
    public static NThought Create(ThoughtModel model, bool starVisible = true, bool toilVisible = true)
    {
        PackedScene scene = ResourceLoader.Load<PackedScene>(_scenePath);
        NThought node = scene.Instantiate<NThought>();
        node.StarVisible = starVisible;
        node.ToilVisible = toilVisible;
        node.Model = model;
        return node;
    }

    public override void _Ready()
    {
        _icon = GetNode<TextureRect>("%Icon");
        _starContainer = GetNode<HBoxContainer>("%StarContainer");
        _star = GetNode<TextureRect>("%Star");
        _overlay = GetNode<Control>("%Overlay");
        _toilContainer = GetNode<Control>("%ToilContainer");
        _label = GetNode<Label>("%Label");
        Reload();
    }

    // 订阅放在进树/出树里：节点常常是“先 new、设 Model、再 AddChild”，
    // 那时 IsInsideTree()=false，光靠 setter 订阅会漏，这里补齐（对照 NCard）。
    public override void _EnterTree()
    {
        SubscribeToModel(Model);
    }

    public override void _ExitTree()
    {
        UnsubscribeFromModel(Model);
    }

    public void SetOverlayVisible(bool visible)
    {
        OverlayVisible = visible;
        if (IsNodeReady())
        {
            _overlay.Visible = visible;
        }
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
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
                    Pressed?.Invoke(this);
                }
            }
        }
    }


    private void SubscribeToModel(ThoughtModel? model)
    {
        if (model != null && IsInsideTree())
        {
            model.ToilChanged += OnToilChanged;
            model.RarityChanged += OnRarityChanged;
        }
    }

    private void UnsubscribeFromModel(ThoughtModel? model)
    {
        if (model != null)
        {
            model.ToilChanged -= OnToilChanged;
            model.RarityChanged -= OnRarityChanged;
        }
    }

    private void OnToilChanged() => UpdateToil();

    private void OnRarityChanged() => UpdateStars();

    private void Reload()
    {
        // setter 可能在 _Ready 之前调到这里（此时节点引用还没拿到），守卫一下
        if (!IsNodeReady() || _model == null)
        {
            return;
        }
        _icon.Texture = _model.Icon;
        _starContainer.Visible = StarVisible;
        _toilContainer.Visible = ToilVisible;
        _overlay.Visible = OverlayVisible;
        UpdateStars();
        UpdateToil();
    }

    private void UpdateToil()
    {
        if (!IsNodeReady() || _model == null)
        {
            return;
        }
        _label.Text = _model.Toil.ToString();
    }

    // Rarity 是几就显示几颗星：以场景里的 _star 为模板，按需复制/删减。
    private void UpdateStars()
    {
        if (!IsNodeReady() || _model == null)
        {
            return;
        }

        int count = Mathf.Max(0, _model.Rarity);

        // 清掉除模板(_star)以外的复制星星
        foreach (Node child in _starContainer.GetChildren())
        {
            if (child != _star)
            {
                _starContainer.RemoveChild(child);
                child.QueueFree();
            }
        }

        // 模板作为第 1 颗（count==0 时隐藏）
        _star.Visible = count >= 1;

        // 再补 count-1 颗
        for (int i = 1; i < count; i++)
        {
            TextureRect star = (TextureRect)_star.Duplicate();
            star.Visible = true;
            _starContainer.AddChild(star);
        }
    }
}
