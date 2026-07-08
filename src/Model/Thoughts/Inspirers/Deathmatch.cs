using MegaCrit.Sts2.Core.Entities.Creatures;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Models.Powers;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 048 死斗：使用后下次战斗所有我方单位攻击速度 +100，获得 5 层覆甲。
public sealed class Deathmatch : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        await ThoughtFx.ApplyToAllAllies<AttackSpeed>(combatState, Owner.Creature, 100);
        await ThoughtFx.ApplyToAllAllies<PlatingPower>(combatState, Owner.Creature, 5);
    }
}