using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Factories;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Thoughts.Plans;

namespace Skz.Ocean.Model.Relics;

// 043 失落之钥：进入第三幕时，立刻获得400金币（一次性；防止诡谲断章返程重入 act 重复触发）
public sealed class LostKey : RelicModel
{
    private bool _triggered;

    public override RelicRarity Rarity => RelicRarity.Rare;

    [SavedProperty]
    public bool Triggered
    {
        get => _triggered;
        set
        {
            AssertMutable();
            _triggered = value;
        }
    }

    public override async Task AfterActEntered()
    {
        if (Triggered || Owner.RunState.CurrentActIndex != 2)
        {
            return;
        }
        Triggered = true;
        Flash();
        await PlayerCmd.GainGold(400, Owner);
    }
}
