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

namespace Skz.Ocean.Model.Monsters;

// 35 破片：debuff(1层易伤) -> 攻击(5*2) -> 逃离
public sealed class Scrap : SarkazMonsterModel
{
    public override int MaxInitialHp => 20;
    public override int MinInitialHp => 20;

    private const int _damage = 5;
    private const int _repeat = 2;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MinionPower>(new ThrowingPlayerChoiceContext(), Creature, 1m, null, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new MultiAttackIntent(_damage, _repeat), new DebuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new MultiAttackIntent(_damage, _repeat));
        MoveState escape = new MoveState("ESCAPE", Escape, new EscapeIntent());
        debuff.FollowUpState = attack;
        attack.FollowUpState = escape;
        escape.FollowUpState = escape;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff, attack, escape }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).WithHitCount(_repeat).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).WithHitCount(_repeat).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Escape(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.Escape(Creature);
    }
}
