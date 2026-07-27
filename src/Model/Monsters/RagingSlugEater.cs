using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Monsters;

// 21 暴走食虫花：可消耗牌(非手牌) > 3 时 技能(消耗随机一张非手牌) -> 不行动 循环；
// 当可消耗牌 ≤ 3 时改为攻击(6)。
public sealed class RagingSlugEater : SarkazMonsterModel
{
    public override int MaxInitialHp => 21;
    public override int MinInitialHp => 21;

    private const int _damage = 6;

    private List<CardModel> Consumable()
    {
        List<CardModel> cards = new List<CardModel>();
        foreach (Player player in Creature.CombatState.Players)
        {
            cards.AddRange(PileType.Draw.GetPile(player).Cards);
            cards.AddRange(PileType.Discard.GetPile(player).Cards);
        }
        return cards;
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState devour = new MoveState("DEVOUR", Devour, new DebuffIntent());
        MoveState idle = new MoveState("IDLE", Idle, new SleepIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        ConditionalBranchState branch = new ConditionalBranchState("EATER_BRANCH");
        branch.AddState(devour, () => Consumable().Count > 3);
        branch.AddState(attack, () => true);
        devour.FollowUpState = idle;
        idle.FollowUpState = branch;
        attack.FollowUpState = branch;
        return new MonsterMoveStateMachine(
            new List<MonsterState> { branch, devour, idle, attack }, branch);
    }

    private async Task Devour(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "AttackBegin", 0.5f);
        List<CardModel> cards = Consumable();
        if (cards.Count == 0) return;
        CardModel victim = RunRng.MonsterAi.NextItem(cards);
        await CardCmd.Exhaust(new ThrowingPlayerChoiceContext(), victim);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("AttackBegin", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState attackIdle = new AnimState("Attack_Idle");
        AnimState attackBegin = new AnimState("Attack_Begin");
        AnimState attackEnd = new AnimState("Attack_End");
        AnimState die = new AnimState("Die");
        attackBegin.NextState = attackIdle;
        attackIdle.NextState = attackEnd;
        attackEnd.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("AttackBegin", attackBegin);
        animator.AddAnyState("Dead", die);
        return animator;
    }
}
