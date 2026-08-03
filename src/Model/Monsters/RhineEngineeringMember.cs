using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 42 莱茵生命工程科成员：buff(力量+2) -> 治疗(所有敌方回30%最大生命) -> 攻击(13) 循环
public sealed class RhineEngineeringMember : SarkazMonsterModel
{
    public override int MaxInitialHp => 35;
    public override int MinInitialHp => 35;

    private const int _damage = 13;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState heal = new MoveState("HEAL", Heal, new HealIntent());
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        buff.FollowUpState = heal;
        heal.FollowUpState = attack;
        attack.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { buff, heal, attack }, buff);
    }

    private async Task Heal(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Skill", 0f);
        foreach (Creature enemy in Creature.CombatState.Enemies)
        {
            if (!enemy.IsAlive) continue;
            decimal amount = enemy.MaxHp * 30m / 100m;
            if (amount > 0) await CreatureCmd.Heal(enemy, amount);
        }
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
