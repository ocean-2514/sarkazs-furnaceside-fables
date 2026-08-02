using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 51 蒸汽战车：生命值首次低于50%时移速+100%，+100 AttackSpeed(仅一次)；
// buff(2层SkzAfterimagePower) -> 攻击1(5) -> 攻击2(1*6) 循环
public sealed class SteamTank : SarkazMonsterModel
{
    public override int MaxInitialHp => 64;
    public override int MinInitialHp => 64;

    private const int _damage1 = 5;
    private const int _damage2 = 1;
    private const int _repeat2 = 6;

    private bool _triggered;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (_triggered || target != Creature || Creature.IsDead) return;
        if ((decimal)Creature.CurrentHp <= (decimal)Creature.MaxHp / 2m)
        {
            _triggered = true;
            BaseSpeedBonus = InitialSpeed; // 移速+100%
            await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), Creature, 100, Creature, null);
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        buff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { buff, attack1, attack2 }, buff);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<SkzAfterimagePower>(new ThrowingPlayerChoiceContext(), Creature, 2, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
