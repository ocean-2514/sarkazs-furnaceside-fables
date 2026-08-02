using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 64 帝国炮火中枢先兆者：不行动 -> 攻击(随机18-33) 循环
public sealed class ImperialArtilleryCoreTargeteer : SarkazMonsterModel
{
    public override int MaxInitialHp => 98;
    public override int MinInitialHp => 98;

    public override bool Blockable => false;


    private const int _minDamage = 18;
    private const int _maxDamage = 33;

    private int _rolledDamage = _minDamage;

    private void RollDamage() =>
        _rolledDamage = _minDamage + RunRng.MonsterAi.NextInt(_maxDamage - _minDamage + 1);

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        RollDamage();
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        await base.AfterSideTurnEnd(choiceContext, side, participants);
        if (side == Creature.Side) RollDamage();
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new MoveState("IDLE", Idle, new UnknownIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(() => _rolledDamage));
        idle.FollowUpState = attack;
        attack.FollowUpState = idle;
        return new MonsterMoveStateMachine(new List<MonsterState> { idle, attack }, idle);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_rolledDamage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
