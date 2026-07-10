using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Acts;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Relics.Base;

// “仅在诡谲断章中生效的收藏品”标记（论断系列按它计数）。
public interface IFragmentMystery
{
}

// 之谜系列基类（158~165）：Event 稀有度、只在诡谲断章内生效、
// 公共代价——所有敌人生命值+10%（按敌人逐个登记，覆盖中途入场）。
public abstract class FragmentMysteryRelic : RelicModel, IFragmentMystery
{
    private readonly HashSet<Creature> _hpApplied = new HashSet<Creature>();
    private readonly HashSet<Creature> _enemyRegistered = new HashSet<Creature>();

    public override RelicRarity Rarity => RelicRarity.Event;

    protected bool InFragment => Owner?.RunState?.Act is BizarreFragment;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (side != Owner.Creature.Side) {
            return;
        }
        foreach (Creature enemy in combatState.Enemies.ToList())
        {
            if (enemy == null || !enemy.IsAlive || _hpApplied.Contains(enemy))
            {
                continue;
            }
            _hpApplied.Add(enemy);
            await CreatureCmd.GainMaxHp(enemy, enemy.MaxHp * 20 / 100);
            await SarkazRelicFx.ApplyPower<AttackPower>(this, enemy, 10);
        }
        if (!InFragment)
        {
            return;
        }
        foreach (Creature enemy in combatState.Enemies.ToList())
        {
            if (enemy == null || !enemy.IsAlive || _enemyRegistered.Contains(enemy))
            {
                continue;
            }
            _enemyRegistered.Add(enemy);
            await OnEnemyRegistered(enemy);
        }
        await OnPlayerTurnStartInFragment(combatState);
    }

    // 子类：对每个（新登记的）敌人的一次性附加效果
    protected virtual Task OnEnemyRegistered(Creature enemy)
    {
        return Task.CompletedTask;
    }

    // 子类：诡谲断章内每个我方回合开始的效果
    protected virtual Task OnPlayerTurnStartInFragment(ICombatState combatState)
    {
        return Task.CompletedTask;
    }

    public override Task AfterCombatEnd(CombatRoom room)
    {
        _hpApplied.Clear();
        return Task.CompletedTask;
    }
}
