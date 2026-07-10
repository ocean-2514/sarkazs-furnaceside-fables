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

// 036 支柱-典训：升级一张能力牌
public sealed class StalwartAidTraining : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Common;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await SarkazRelicFx.UpgradeCardsViaSelection(Owner, 1, c => c.Type == CardType.Power);
    }
}
