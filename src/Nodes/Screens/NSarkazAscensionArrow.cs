namespace Skz.Ocean.Nodes.Screens;

using Godot;

public partial class NSarkazAscensionArrow : Control
{
    [Signal]
    public delegate void PressedEventHandler();

    public override void _GuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton mouseButton
            || mouseButton.ButtonIndex != MouseButton.Left
            || !mouseButton.Pressed)
        {
            return;
        }

        EmitSignal(SignalName.Pressed);
        AcceptEvent();
    }
}
