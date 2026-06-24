using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 036 兴亡：使用后下次战斗所有我方获得 6 点力量，最大生命值 +50%（仅本场）。
public sealed class RiseAndFall : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;

    private Dictionary<Creature, int> _granted;

    public override async Task AfterSideTurnStart(CombatSide side, CombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        await ThoughtFx.ApplyToAllAllies<StrengthPower>(combatState, Owner.Creature, 6);
        _granted = await ThoughtFx.GainTempMaxHpPercent(combatState, 0.50);
    }

    public override async Task AfterCombatEnd(CombatRoom room)
    {
        await ThoughtFx.LoseTempMaxHp(_granted);
        _granted = null;
        await base.AfterCombatEnd(room);
    }
}