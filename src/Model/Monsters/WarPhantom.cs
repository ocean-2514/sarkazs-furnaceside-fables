using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 37 战争幽灵：生命值降到一半以下时立即召唤铁砧(仅一次)；
// 召唤(破片Scrap) -> 攻击1(10) -> 攻击2(6*2) 循环
public sealed class WarPhantom : SarkazMonsterModel
{
    public override int MaxInitialHp => 55;
    public override int MinInitialHp => 55;

    private const int _damage1 = 10;
    private const int _damage2 = 6;
    private const int _repeat2 = 2;

    private bool _summonedAnvil;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (_summonedAnvil || target != Creature || Creature.IsDead) return;
        if ((decimal)Creature.CurrentHp <= (decimal)Creature.MaxHp / 2m)
        {
            _summonedAnvil = true;
            await CreatureCmd.Add(ModelDb.Monster<Anvil>().ToMutable(), CombatState, CombatSide.Enemy, "third");
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState summon = new MoveState("SUMMON", Summon, new SummonIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        summon.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { summon, attack1, attack2 }, summon);
    }

    private async Task Summon(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.Add(ModelDb.Monster<Scrap>().ToMutable(), CombatState, CombatSide.Enemy, "first");
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
