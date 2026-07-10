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

// 123 立体艺术装置：回复20目标生命，每次战斗结束后额外回复4目标生命
public sealed class ThreeDimensionalArtDisplay : RelicModel
{
    public override RelicRarity Rarity => RelicRarity.Uncommon;

    public override bool HasUponPickupEffect => true;

    public override async Task AfterObtained()
    {
        await CreatureCmd.Heal(Owner.Creature, 20);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        Flash();
        await CreatureCmd.Heal(Owner.Creature, 4);
    }
}
