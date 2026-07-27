using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 12 萨卡兹迷宫劝导者：攻击(7) -> 召唤(年代之刺) -> 之后 攻击 -> Debuff(加一张虚空入手牌) 循环
public sealed class SarkazMazeGuide : SarkazMonsterModel
{
    public override int MaxInitialHp => 35;
    public override int MinInitialHp => 35;

    private const int _damage = 7;    

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attackFirst = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        MoveState summon = new MoveState("SUMMON", Summon, new SummonIntent());
        MoveState attackLoop = new MoveState("ATTACK_LOOP", Attack, new SingleAttackIntent(_damage));
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        attackFirst.FollowUpState = summon;
        summon.FollowUpState = debuff;
        debuff.FollowUpState = attackLoop;
        attackLoop.FollowUpState = debuff;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { attackFirst, summon, attackLoop, debuff }, attackFirst);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Summon(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.Add(ModelDb.Monster<SpinesOfEpoch>().ToMutable(), CombatState, 
            CombatSide.Enemy, _dict.GetValueOrDefault(Creature.SlotName, null));
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await CardPileCmd.AddToCombatAndPreview<Void>(targets, PileType.Draw, 1, null);
    }
}
