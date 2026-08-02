using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 59 孽罪奇美拉：移速150；
// 技能(50护盾+25层ConfrontationPower) -> 攻击1(40) -> 攻击2(随机20-38) 循环
public sealed class SacrilegiousChimera : SarkazMonsterModel
{
    public override int MaxInitialHp => 115;
    public override int MinInitialHp => 115;

    public override decimal InitialSpeed => 150;

    private const int _block = 50;
    private const int _confrontation = 25;
    private const int _damage1 = 40;
    private const int _minDamage2 = 20;
    private const int _maxDamage2 = 38;

    private int _rolledDamage2 = _minDamage2;

    private bool _ignited = false;

    public bool Ignited
    {
        get => _ignited;
        set
        {
            _ignited = value;
        }
    }

    private void RollDamage() =>
        _rolledDamage2 = _minDamage2 + RunRng.MonsterAi.NextInt(_maxDamage2 - _minDamage2 + 1);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        RollDamage();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Creature.Side) RollDamage();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState skill = new MoveState("SKILL", Skill, new BuffIntent(), new DebuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(() => _rolledDamage2));
        skill.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = skill;
        return new MonsterMoveStateMachine(new List<MonsterState> { skill, attack1, attack2 }, skill);
    }

    private async Task Skill(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, _block, ValueProp.Move, null);
        await PowerCmd.Apply<ConfrontationPower>(new ThrowingPlayerChoiceContext(), targets, _confrontation, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim(_ignited ? "Attack2" : "Attack1", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage2).FromMonster(this).WithAttackerAnim(_ignited ? "Attack2" : "Attack1", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle1 = new AnimState("Idle_1", isLooping: true);
        AnimState idle2 = new AnimState("Idle_2", isLooping: true);
        AnimState attack1 = new AnimState("Attack_1");
        AnimState attack2 = new AnimState("Attack_2");
        AnimState die = new AnimState("Die_2");
        AnimState skill = new AnimState("Skill");
        attack1.NextState = idle1;
        attack2.NextState = idle2;
        skill.NextState = idle2;
        CreatureAnimator animator = new CreatureAnimator(idle1, controller);
        animator.AddAnyState("Attack1", attack1);
        animator.AddAnyState("Attack2", attack2);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Skill", skill);
        return animator;
    }
}
