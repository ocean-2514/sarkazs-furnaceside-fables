using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Screens.Capstones;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Model;
using Skz.Ocean.Nodes.Screens;

namespace Skz.Ocean.Nodes.TopBar;

public partial class NTopBarThoughtUsed : Control, ITopBarScene
{
    private Player _player;

    private TextureRect _icon;

    public ThoughtModel? Model => _player.GetThoughtUsed();

    public override void _Ready()
    {
        _icon = GetNode<TextureRect>("Icon");
    }

    public void Initialize(Player player)
    {
        _player = player;
        _player.GetExtraData().ThoughtUsedChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        _player.GetExtraData().ThoughtUsedChanged -= Refresh;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (Model == null) return;
        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex : MouseButton.Left})
        {
            ToggleScreen();
        }
    }

    private void ToggleScreen()
    {
        if (_player == null)
        {
            return;
        }
        NCapstoneContainer? container = NCapstoneContainer.Instance;
        if (container == null)
        {
            return;
        }
        if (container.CurrentCapstoneScreen is NOneThoughtViewScreen)
        {
            container.Close();
        }
        else if (container.CurrentCapstoneScreen is NThoughtViewScreen screen)
        {
            if (screen.Overlay != null)
            {
                screen.ChangeOverlay(null);
                return;
            }
            screen.ChangeOverlay(Model);
        }
        else
        {
            NOneThoughtViewScreen.ShowScreen(Model);
        }
    }

    private void Refresh()
    {
        if (Model == null)
        {
            _icon.Texture = null;
        } 
        else
        {
            _icon.Texture = Model.Icon;
        }
    }
}