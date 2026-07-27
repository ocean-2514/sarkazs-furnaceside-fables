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

// 20 萨卡兹枯朽战车：攻击1(5) -> Debuff(施加3层 SkzCorruption) -> 攻击2(3*3) 循环
public sealed class SarkazWitherTank : SarkazMonsterModel
{
    public override int MaxInitialHp => 46;
    public override int MinInitialHp => 46;

    private const int _damage1 = 5;
    private const int _damage2 = 3;
    private const int _repeat2 = 3;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        attack1.FollowUpState = debuff;
        debuff.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, debuff, attack2 }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<SkzCorruptionPower>(new ThrowingPlayerChoiceContext(), targets, 3, Creature, null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
