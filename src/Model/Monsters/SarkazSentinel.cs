using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 23 萨卡兹哨兵：第一次损失生命值后，所有敌方攻击力+30、获得4层覆甲(PlatingPower)、
// 立即激活所有萨卡兹魔剑士。状态机：不行动。
public sealed class SarkazSentinel : SarkazMonsterModel
{
    public override int MaxInitialHp => 30;
    public override int MinInitialHp => 30;

    private const int _attackBuff = 30;
    private const int _plating = 4;

    private bool _triggered;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (_triggered || target != Creature || result.UnblockedDamage <= 0) return;
        _triggered = true;

        await CreatureCmd.TriggerAnim(Creature, "Wake", 0f);

        foreach (Creature enemy in Creature.CombatState.Enemies.ToList())
        {
            if (!enemy.IsAlive) continue;
            await PowerCmd.Apply<AttackPower>(new ThrowingPlayerChoiceContext(), enemy, _attackBuff, Creature, null);
            await PowerCmd.Apply<PlatingPower>(new ThrowingPlayerChoiceContext(), enemy, _plating, Creature, null);
            if (enemy.Monster is SarkazBladeweaver weaver) await weaver.Wake();
            if (enemy.Monster is SarkazWorldcurser worldcurser) await worldcurser.Wake();
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState idle = new MoveState("IDLE", Idle, new SleepIntent());
        idle.FollowUpState = idle;
        return new MonsterMoveStateMachine(new List<MonsterState> { idle }, idle);
    }

    private Task Idle(IReadOnlyList<Creature> targets) => Task.CompletedTask;

    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState skillIdle = new AnimState("Skill_Idle", isLooping: true);
        AnimState skillBegin = new AnimState("Skill_Begin");
        AnimState die = new AnimState("Die");
        skillBegin.NextState = skillIdle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Wake", skillBegin);
        animator.AddAnyState("Dead", die);
        return animator;
    }
}
