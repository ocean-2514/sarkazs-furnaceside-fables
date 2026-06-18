using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using Skz.Ocean.Entities.Players;

namespace Skz.Ocean.Nodes.TopBar;

public partial class NTopBarPlan : Control, ITopBarScene
{
    private Player _player;

    private Label _label;

    public override void _Ready()
    {
        _label = GetNode<Label>("%Label");


    }

    public void Initialize(Player player)
    {
        _player = player;
        _player.GetExtraData().PlayerThoughtChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        _player.GetExtraData().PlayerThoughtChanged -= Refresh;
    }

    private void Refresh()
    {
        _label.Text = _player.GetPlans().Count.ToString();
    } 
}