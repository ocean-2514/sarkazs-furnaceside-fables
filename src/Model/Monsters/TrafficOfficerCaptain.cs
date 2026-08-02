using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 69 "交通警长"：加入战场的前两回合获得1层LurkPower，移速+100；
// 攻击1(随机15-30 + 1层易伤) -> 攻击2(随机10-20 *2) 循环
public sealed class TrafficOfficerCaptain : SarkazMonsterModel
{
    public override int MaxInitialHp => 76;
    public override int MinInitialHp => 76;

    public override decimal InitialSpeed => 250;

    private const int _minDamage1 = 15;
    private const int _maxDamage1 = 30;
    private const int _minDamage2 = 10;
    private const int _maxDamage2 = 20;
    private const int _repeat2 = 2;

    private int _rolledDamage1 = _minDamage1;
    private int _rolledDamage2 = _minDamage2;

    private int _turnsElapsed;

    private void RollDamage()
    {
        _rolledDamage1 = _minDamage1 + RunRng.MonsterAi.NextInt(_maxDamage1 - _minDamage1 + 1);
        _rolledDamage2 = _minDamage2 + RunRng.MonsterAi.NextInt(_maxDamage2 - _minDamage2 + 1);
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        RollDamage();
        BaseSpeedBonus = 100;
        await PowerCmd.Apply<LurkPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Creature.Side)
        {
            _turnsElapsed++;
            RollDamage();
            if (_turnsElapsed >= 2)
            {
                BaseSpeedBonus = 0;
                if (Creature.HasPower<LurkPower>())
                    await PowerCmd.Remove(Creature.GetPower<LurkPower>());
            }
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1,
            new SingleAttackIntent(() => _rolledDamage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2,
            new MultiAttackIntent(_minDamage2, _repeat2));
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, attack2 }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
        await DamageCmd.Attack(_rolledDamage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
