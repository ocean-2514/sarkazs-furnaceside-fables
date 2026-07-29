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

// 24 高能源石虫：加入战斗后获得5层DeathBlast；攻击(8) -> buff(再+4 DeathBlast) 循环
public sealed class InfusedOriginiumSlug : SarkazMonsterModel
{
    public override int MaxInitialHp => 24;
    public override int MinInitialHp => 24;

    private const int _damage = 8;
    private const int _extraBlast = 4;
    private const int _initialBlast = 5;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<DeathBlast>(new ThrowingPlayerChoiceContext(), Creature, _initialBlast, null, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        attack.FollowUpState = buff;
        buff.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack, buff }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<DeathBlast>(new ThrowingPlayerChoiceContext(), Creature, _extraBlast, Creature, null);
    }
}
