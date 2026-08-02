using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 32 游击队萨卡兹术师：加入战斗后获得4层AngerPower；第一回合debuff(施加RestraintPower1) -> 攻击(随机7-18) 循环
public sealed class SarkazGuerrillaCaster : SarkazMonsterModel
{
    public override int MaxInitialHp => 52;
    public override int MinInitialHp => 52;

    private const int _minDamage = 7;
    private const int _maxDamage = 18;

    private int _rolledDamage = _minDamage;

    private void RollDamage() =>
        _rolledDamage = _minDamage + RunRng.MonsterAi.NextInt(_maxDamage - _minDamage + 1);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        RollDamage();
        await PowerCmd.Apply<AngerPower>(new ThrowingPlayerChoiceContext(), Creature, 4, Creature, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Creature.Side) RollDamage();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(() => _rolledDamage));
        debuff.FollowUpState = attack;
        attack.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff, attack }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<RestraintPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage).FromMonster(this)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
