using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 047 爆破：使用后下次战斗年代之刺最大生命值 -50%。
// 暂不实现：相关敌人(年代之刺)尚未制作，先排除出奖励池(None)。
public sealed class Demolition : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    public override async Task AfterSideTurnStartLate(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (Status != ThoughtStatus.Used) return;
        if (side != Owner.Creature.Side || combatState.RoundNumber != 1) return;
        foreach (Creature creature in combatState.Enemies)
        {
            if (creature.Monster is SpinesOfEpoch)
            {
                await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), creature, creature.MaxHp / 2, null);
            }
        }
    }

}
