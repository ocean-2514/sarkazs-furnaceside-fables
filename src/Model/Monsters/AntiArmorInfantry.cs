using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 34 反装甲步兵：第一次攻击过后移速+100；攻击1(12) -> debuff(减1敏捷) -> 攻击2(3*2) 循环
public sealed class AntiArmorInfantry : SarkazMonsterModel
{
    public override int MaxInitialHp => 33;
    public override int MinInitialHp => 33;

    public override decimal InitialSpeed => 200;
    private const int _speedBonus = 100;

    private const int _damage1 = 12;
    private const int _damage2 = 3;
    private const int _repeat2 = 2;

    private bool _attacked;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        attack1.FollowUpState = debuff;
        debuff.FollowUpState = attack2;
        attack2.FollowUpState = debuff;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, debuff, attack2 }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack1", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        _attacked = true;
        BaseSpeedBonus = _speedBonus;
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<DexterityPower>(new ThrowingPlayerChoiceContext(), targets, -1, Creature, null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack2", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle1 = new AnimState("Idle_A", isLooping: true);
        AnimState idle2 = new AnimState("Idle_B", isLooping: true);
        AnimState attack1 = new AnimState("Attack_A");
        AnimState attack2 = new AnimState("Attack_B");
        AnimState die = new AnimState("Die_B");
        attack1.NextState = idle2;
        attack2.NextState = idle1;
        CreatureAnimator animator = new CreatureAnimator(idle1, controller);
        animator.AddAnyState("Attack1", attack1);
        animator.AddAnyState("Attack2", attack2);
        animator.AddAnyState("Dead", die);
        return animator;
    }
}
