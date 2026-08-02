using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 60 萨卡兹悖谬暴虐兵长：不可阻挡，移速150；
// 攻击1(30) -> buff(25层HardToKillPower) -> 之后攻击2(18*2) -> debuff(1层脆弱) -> 攻击1 循环
public sealed class SarkazAbsurdistTyrantCaptain : SarkazMonsterModel
{
    public override int MaxInitialHp => 115;
    public override int MinInitialHp => 115;

    public override decimal InitialSpeed => 150;

    public override bool Blockable => false;

    private const int _damage1 = 30;
    private const int _hardToKill = 25;
    private const int _damage2 = 18;
    private const int _repeat2 = 2;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<UnblockablePower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1First = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack1Loop = new MoveState("ATTACK1_LOOP", Attack1, new SingleAttackIntent(_damage1));
        attack1First.FollowUpState = buff;
        buff.FollowUpState = attack2;
        attack2.FollowUpState = debuff;
        debuff.FollowUpState = attack1Loop;
        attack1Loop.FollowUpState = attack2;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { attack1First, buff, attack2, debuff, attack1Loop }, attack1First);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<HardToKillPower>(new ThrowingPlayerChoiceContext(), Creature, _hardToKill, Creature, null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<FrailPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }
}
