using System;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using Skz.Ocean.Entities.Players;

namespace Skz.Ocean.Nodes.Thoughts;

public partial class NToilIndicator : Control
{
    private Player _player;

    private const float _maxBarLength = 430f;
    private const string LucidBarColor = "#009d77";
    private const string FracturedBarColor = "#bc8224";
    private const string ObstructedBarColor = "#b91615";

    private Label _lucidLimit;
    private Label _fracturedLimit;
    private ColorRect _bar;

    public override void _Ready()
    {
        _lucidLimit = GetNode<Label>("LucidLimit");
        _fracturedLimit = GetNode<Label>("FracturedLimit");
        _bar = GetNode<ColorRect>("%Bar");
    }

    public void Initialize(Player player)
    {
        _player = player;
        _player.GetExtraData().ToilLimitChanged += Refresh;
        _player.GetExtraData().PlayerThoughtChanged += Refresh;
        Refresh();
    }

    public override void _ExitTree()
    {
        if (_player != null)
        {
            _player.GetExtraData().ToilLimitChanged -= Refresh;
            _player.GetExtraData().PlayerThoughtChanged -= Refresh;
        }
    }

    private void Refresh()
    {
        int lucidLimit = _player.GetLucidLimit();
        int fracturedLimit = _player.GetFracturedLimit();
        int totalToil = _player.GetTotalToil();
        MindState mindState = _player.GetMindState();

        _lucidLimit.Text = lucidLimit.ToString();
        _fracturedLimit.Text = fracturedLimit.ToString();

        string barColor = "";
        switch (mindState)
        {
            case MindState.Lucid:
                barColor = LucidBarColor;
                break;
            case MindState.Fractured:
                barColor = FracturedBarColor;
                break;
            case MindState.Obstructed:
                barColor = ObstructedBarColor;;
                break;
        }
        _bar.Color = Color.FromString(barColor, Colors.White);
        float ratio = lucidLimit > 0 ? Math.Min(1f, (float)totalToil / fracturedLimit) : 1f;
        _bar.Size = new Vector2(_maxBarLength * ratio, _bar.Size.Y);
    }
}