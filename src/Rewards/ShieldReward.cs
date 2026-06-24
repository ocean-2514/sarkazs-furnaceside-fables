using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.TestSupport;
using Skz.Ocean.Entities.Players;

namespace Skz.Ocean.Rewards;

public class ShieldReward : Reward
{
    private int _shield = 0;

    public ShieldReward(Player player, int amount) : base(player)
    {
        _shield = amount;
    }

    public override int RewardsSetIndex => 5;

    protected override RewardType RewardType => RewardType.Gold;

    public override bool IsPopulated => _shield > 0;

    public override LocString Description 
    {
        get
        {
            LocString locString = new LocString("gameplay_ui", "SHIELD");
            locString.Add("shield", _shield);
            return locString;
        }
    }

    protected override async Task<bool> OnSelect()
    {
        Player.GainShield(_shield);
        return true;
    }

    public override Task Populate()
    {
        return Task.CompletedTask;
    }

    public override Control? CreateIcon()
    {
        if (TestMode.IsOn || _shield == 0) return null;
        TextureRect icon = new TextureRect
        {
            Texture = ResourceLoader.Load<Texture2D>("res://images/ui/shield.png"),
            ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
        };
        icon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        return icon;
    }

    public override void MarkContentAsSeen()
    {
    }
}