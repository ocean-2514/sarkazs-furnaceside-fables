using Godot;

namespace Skz.Ocean.Nodes;

public partial class NDragScrollContainer : ScrollContainer
{
    private bool _dragging;

    // 用 _Input 而不是 _GuiInput：这样即便鼠标压在子节点(思绪条目)上，
    // 也能拿到事件（子节点的 mouse_filter 不影响）。
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseButton mb && mb.ButtonIndex == MouseButton.Left)
        {
            // 只有当按下点落在本容器范围内时才开始拖动
            _dragging = mb.Pressed && GetGlobalRect().HasPoint(mb.GlobalPosition);
        }
        else if (@event is InputEventMouseMotion mm && _dragging)
        {
            ScrollVertical -= (int)mm.Relative.Y;
        }
    }
}
