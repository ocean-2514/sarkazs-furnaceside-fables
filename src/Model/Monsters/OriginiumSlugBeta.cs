using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 3 源石虫·β：攻击循环，攻击 4
public sealed class OriginiumSlugBeta : SarkazMonsterModel
{
    public override int MaxInitialHp => 9;
    public override int MinInitialHp => 9;

    private const int _damage = 4;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        attack.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
