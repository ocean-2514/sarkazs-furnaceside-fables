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

// 29 潜伏者：加入战斗后获得1层LurkPower；攻击1(3*2+脆弱1) -> buff(无实体1) -> 攻击2(7) 循环
public sealed class Lurker : SarkazMonsterModel
{
    public override int MaxInitialHp => 20;
    public override int MinInitialHp => 20;

    private const int _damage1 = 3;
    private const int _repeat1 = 2;
    private const int _damage2 = 7;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<LurkPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new MultiAttackIntent(_damage1, _repeat1), new DebuffIntent());
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        attack1.FollowUpState = buff;
        buff.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, buff, attack2 }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).WithHitCount(_repeat1).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
