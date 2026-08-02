using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 63 碎骨：debuff(1层SkullshatterPower) -> 攻击1(25) -> 攻击2(10+1脆弱) -> 攻击3(6*5) 循环
public sealed class Skullshatterer : SarkazMonsterModel
{
    public override int MaxInitialHp => 350;
    public override int MinInitialHp => 350;

    public override decimal InitialSpeed => 150;

    private const int _damage1 = 25;
    private const int _damage2 = 20;
    private const int _damage3 = 6;
    private const int _repeat3 = 5;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<BorrowShieldPower>(new ThrowingPlayerChoiceContext(), Creature, 30, null, null);
    }


    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        MoveState attack3 = new MoveState("ATTACK3", Attack3, new MultiAttackIntent(_damage3, _repeat3));
        debuff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack3;
        attack3.FollowUpState = debuff;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { debuff, attack1, attack2, attack3 }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<SkullshatterPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack3(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage3).WithHitCount(_repeat3).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
