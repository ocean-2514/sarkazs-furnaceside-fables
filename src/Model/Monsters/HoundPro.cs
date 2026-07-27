using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 6 猎狗pro：移速 350，攻击循环，攻击 3*2
public sealed class HoundPro : SarkazMonsterModel
{
    public override int MaxInitialHp => 23;
    public override int MinInitialHp => 23;

    public override decimal InitialSpeed => 350;

    private const int _damage = 3;
    private const int _hits = 2;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new MultiAttackIntent(_damage, _hits));
        attack.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).WithHitCount(_hits).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
