using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using Skz.Ocean.Entities.Players;

namespace Skz.Ocean.Nodes.TopBar;

public partial class NTopBarShield : Control, ITopBarScene
{
    private Player _player;

    private Label _label;

    public override void _Ready()
    {
        _label = GetNode<Label>("%Label");
    }

    public override void _ExitTree()
    {
        _player.GetExtraData().ShieldChanged -= Refresh;
    }

    public void Initialize(Player player)
    {
        _player = player;
        player.GetExtraData().ShieldChanged += Refresh;
        Refresh();
    }

    private void Refresh()
    {
        _label.Text = _player.GetShield().ToString();
    }
}