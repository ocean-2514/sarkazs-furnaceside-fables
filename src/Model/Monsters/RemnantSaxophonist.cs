using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 5 残党萨克斯手：Buff(力量+1) -> 技能(攻击14 + 易伤1) -> 攻击(8) 循环
public sealed class RemnantSaxophonist : SarkazMonsterModel
{
    public override int MaxInitialHp => 43;
    public override int MinInitialHp => 43;

    private const int _skillDamage = 14;
    private const int _attackDamage = 8;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState skill = new MoveState("SKILL", Skill, new SingleAttackIntent(_skillDamage), new DebuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_attackDamage));
        buff.FollowUpState = skill;
        skill.FollowUpState = attack;
        attack.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { buff, skill, attack }, buff);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    private async Task Skill(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_skillDamage).FromMonster(this).WithAttackerAnim("Skill", 0f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_attackDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
