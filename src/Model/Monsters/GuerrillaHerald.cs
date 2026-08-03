using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 17 游击队传令兵：在场时所有敌人攻击力+10(光环)；
// Buff(给所有敌方5点格挡) -> Heal(所有敌人回2血 + 2层活力) -> 攻击(10) 循环
public sealed class GuerrillaHerald : SarkazMonsterModel
{
    public override int MaxInitialHp => 30;
    public override int MinInitialHp => 30;

    private const int _aura = 10;
    private const int _block = 5;
    private const int _heal = 5;
    private const int _vigor = 2;
    private const int _damage = 10;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        foreach (Creature enemy in Creature.CombatState.Enemies)
        {
            await PowerCmd.Apply<AttackPower>(new ThrowingPlayerChoiceContext(), enemy, _aura, Creature, null);
        }
    }

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Creature) return;
        await RemoveAura(creature.CombatState);
    }

    // ISarkazModel：Escape 前收回光环（基类 AfterSideTurnEnd → SarkazHook → 这里）
    public override async Task BeforeMonsterEscape(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster)
    {
        if (monster != this) return;
        await RemoveAura(combatState);
    }

    private async Task RemoveAura(ICombatState combatState)
    {
        foreach (Creature enemy in combatState.Enemies)
        {
            if (enemy == Creature || !enemy.IsAlive) continue;
            await PowerCmd.Apply<AttackPower>(new ThrowingPlayerChoiceContext(), enemy, -_aura, Creature, null);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState buff = new MoveState("BUFF", Buff, new DefendIntent());
        MoveState heal = new MoveState("HEAL", Heal, new HealIntent(), new BuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        buff.FollowUpState = heal;
        heal.FollowUpState = attack;
        attack.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { buff, heal, attack }, buff);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        foreach (Creature enemy in Creature.CombatState.Enemies)
        {
            if (enemy.IsAlive) await CreatureCmd.GainBlock(enemy, _block, ValueProp.Move, null);
        }
    }

    private async Task Heal(IReadOnlyList<Creature> targets)
    {
        foreach (Creature enemy in Creature.CombatState.Enemies)
        {
            if (!enemy.IsAlive) continue;
            await CreatureCmd.Heal(enemy, _heal);
            await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), enemy, _vigor, Creature, null);
        }
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
