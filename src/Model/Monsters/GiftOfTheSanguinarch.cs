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

// 31 大君之赐：加入战斗后获得1层SanguinarchPower + 50层DamageReductionPower；
// 攻击1(10) -> buff(4层RitualPower) -> 攻击2(5) 循环
public sealed class GiftOfTheSanguinarch : SarkazMonsterModel
{
    public override int MaxInitialHp => 30;
    public override int MinInitialHp => 30;

    private const int _damage1 = 10;
    private const int _ritual = 4;
    private const int _damage2 = 9;
    private const int _drAmount = 50;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<SanguinarchPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<DamageReductionPower>(new ThrowingPlayerChoiceContext(), Creature, _drAmount, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        attack1.FollowUpState = buff;
        buff.FollowUpState = attack2;
        attack2.FollowUpState = attack2;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, buff, attack2 }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<RitualPower>(new ThrowingPlayerChoiceContext(), Creature, _ritual, Creature, null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
