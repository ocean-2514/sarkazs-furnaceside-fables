using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 55 残党乐团大提琴家：加入战斗后获得5层MelancholicCelloPower；
// 攻击1(18) -> 攻击2(28) -> 治疗(回复15，消除负面power) 循环
public sealed class RemnantOrchestraCellist : SarkazMonsterModel
{
    public override int MaxInitialHp => 85;
    public override int MinInitialHp => 85;

    private const int _damage1 = 18;
    private const int _damage2 = 28;
    private const int _heal = 15;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MelancholicCelloPower>(new ThrowingPlayerChoiceContext(), Creature, 5, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        MoveState heal = new MoveState("HEAL", Heal, new HealIntent());
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = heal;
        heal.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, attack2, heal }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Heal(IReadOnlyList<Creature> targets)
    {
        // 回复自身15点
        await CreatureCmd.Heal(Creature, _heal);

        // 消除身上所有负面的power
        // buff且Amount<0 或 debuff且Amount>0
        var toRemove = new List<PowerModel>();
        foreach (PowerModel power in Creature.Powers)
        {
            if (power is MelancholicCelloPower || power is PassionateCelloPower) continue; // 不消除自己的核心power
            if (power is AttackPower || power is AttackSpeed) continue; // 不消除自己的核心power
            if (power.Type == PowerType.Buff && power.Amount < 0)
                toRemove.Add(power);
            else if (power.Type == PowerType.Debuff && power.Amount > 0)
                toRemove.Add(power);
        }
        foreach (PowerModel power in toRemove)
        {
            await PowerCmd.Remove(power);
        }
    }
}
