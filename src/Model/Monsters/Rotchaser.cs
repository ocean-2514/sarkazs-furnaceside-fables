using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 49 逐腐兽：debuff(2层BleedPower) -> 攻击1(8*2) -> 攻击2(16) 循环
public sealed class Rotchaser : SarkazMonsterModel
{
    public override int MaxInitialHp => 43;
    public override int MinInitialHp => 43;

    private const int _damage1 = 8;
    private const int _repeat1 = 2;
    private const int _damage2 = 16;

    private int _bleedPower = 3;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new MultiAttackIntent(_damage1, _repeat1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        debuff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = debuff;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff, attack1, attack2 }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<BleedPower>(new ThrowingPlayerChoiceContext(), targets, _bleedPower, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).WithHitCount(_repeat1).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
