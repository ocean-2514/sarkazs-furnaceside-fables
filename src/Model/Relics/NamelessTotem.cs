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

// 044 无名图腾：立刻获得3缕不同种类的思绪（灵感/遗愿/构想各一）
public sealed class NamelessTotem : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        ThoughtModel inspirer = ThoughtFactory.CreateRandomInspirer(Owner, Owner.PlayerRng.Rewards);
        if (inspirer != null) await ThoughtCmd.Obtain(inspirer, Owner);
        ThoughtModel linger = ThoughtFactory.CreateRandomLinger(Owner, Owner.PlayerRng.Rewards);
        if (linger != null) await ThoughtCmd.Obtain(linger, Owner);
        await ThoughtCmd.Obtain<Plan>(Owner);
    }
}
