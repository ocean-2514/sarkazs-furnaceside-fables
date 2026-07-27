using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

public sealed class RadioactiveOriginiumSlug : SarkazMonsterModel
{
    public override int MaxInitialHp => 18;
    public override int MinInitialHp => 18;

    private const int _attackDamage1 = 4;
    private const int _attackDamage2 = 1;
    private const int _repeat = 3;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<DeathBlast>(new ThrowingPlayerChoiceContext(), Creature, 8, null, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        List<MonsterState> states = new List<MonsterState>();

        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_attackDamage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_attackDamage2, _repeat));
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack1;

        states.Add(attack1);
        states.Add(attack2);

        return new MonsterMoveStateMachine(states, attack1); 
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_attackDamage1).FromMonster(this)
            .WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }
    
    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_attackDamage2).WithHitCount(_repeat).FromMonster(this)
            .OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt")
            .Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState animState = new AnimState("Idle", isLooping: true);
		AnimState animState2 = new AnimState("Attack");
		AnimState animState3 = new AnimState("Die");

		animState2.NextState = animState;

		CreatureAnimator creatureAnimator = new CreatureAnimator(animState, controller);
		creatureAnimator.AddAnyState("Dead", animState3);
		creatureAnimator.AddAnyState("Attack", animState2);

		return creatureAnimator;
    }
}