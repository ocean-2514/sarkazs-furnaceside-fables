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

// 069 未知仪器：基础生命值+25%
public sealed class UnknownInstrument : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Rare;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await SarkazRelicFx.GainBaseHpPercent(Owner, 25);
    }
}
