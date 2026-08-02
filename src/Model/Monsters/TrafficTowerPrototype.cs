using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 68 "交通亭"试做型：移速150，不可阻挡，加入战斗后获得1层SlowPower；
// 死亡后召唤一个"交通警长"；
// buff(自己+20AttackPower, 给玩家20层InjuredPower) -> 攻击(随机30-40, 将3张伤口加入抽牌堆) 循环
public sealed class TrafficTowerPrototype : SarkazMonsterModel
{
    public override int MaxInitialHp => 450;
    public override int MinInitialHp => 450;

    public override decimal InitialSpeed => 150;

    public override bool Blockable => false;

    private const int _attackBuff = 20;
    private const int _injured = 20;
    private const int _minDamage = 30;
    private const int _maxDamage = 40;

    private int _rolledDamage = _minDamage;

    private void RollDamage() =>
        _rolledDamage = _minDamage + RunRng.MonsterAi.NextInt(_maxDamage - _minDamage + 1);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        RollDamage();
        await PowerCmd.Apply<UnblockablePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
        await PowerCmd.Apply<SlowPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Creature.Side) RollDamage();
    }

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Creature) return;
        await CreatureCmd.Add(ModelDb.Monster<TrafficOfficerCaptain>().ToMutable(), creature.CombatState,
            CombatSide.Enemy, Creature.SlotName);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent(), new DebuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(() => _rolledDamage));
        buff.FollowUpState = attack;
        attack.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { buff, attack }, buff);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<AttackPower>(new ThrowingPlayerChoiceContext(), Creature, _attackBuff, Creature, null);
        await PowerCmd.Apply<InjuredPower>(new ThrowingPlayerChoiceContext(), targets, _injured, Creature, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        await CardPileCmd.AddToCombatAndPreview<Wound>(targets, PileType.Draw, 3, null);
    }
}
