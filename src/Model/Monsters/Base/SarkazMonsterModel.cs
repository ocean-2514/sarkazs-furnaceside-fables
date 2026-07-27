using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Animation;
using MegaCrit.Sts2.Core.Bindings.MegaSpine;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Hooks;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters.Base;

// 带"位置/移速"的萨卡兹怪物基类：每回合结束按移速移动，
// 到达 Destination 后对全体玩家造成可格挡伤害并逃离战斗。
// 战斗中途不存档（SL 会整场重打），位置/移速无需 [SavedProperty]。
public abstract class SarkazMonsterModel : MonsterModel
{
    // 注意：以下虚属性在基类构造器里被读取（派生类构造器尚未执行），
    // 子类重写必须是常量表达式，不能依赖实例字段。
    public virtual decimal InitialSpeed => 200;

    public virtual decimal InitialLocation => 0;

    public virtual decimal Destination => 1000;

    // 到达终点逃离时对每名玩家造成的可格挡伤害
    public virtual decimal EscapeDamage => 5m;

    public bool Blockable = true;

    protected readonly Dictionary<string, string> _dict = new Dictionary<string, string>{
        {"first", "forth"},
        {"second", "fifth"},
        {"third", "sixth"}
    };

    // 当前移速 = 基础移速 × 各监听者乘数（减速/加速等持续效果走这里，实时生效；
    // 未入战斗时无法遍历监听者，退回基础移速）。
    public decimal CurrentSpeed
    {
        get
        {
            ICombatState combatState = Creature?.CombatState;
            if (combatState == null) return InitialSpeed;
            return SarkazHook.ModifyMoveSpeed(combatState.RunState, combatState, this, InitialSpeed);
        }
    }

    protected decimal _currentLocation;

    public decimal CurrentLocation
    {
        get => _currentLocation;
        set
        {
            AssertMutable();
            _currentLocation = Math.Clamp(value, InitialLocation, Destination);
        }
    }

    public int CalculatedLeftTurn
    {
        get
        {
            if (CurrentSpeed <= 0) return 99;
            return (int)Math.Ceiling((Destination - _currentLocation) / CurrentSpeed);
        }
    }

    protected SarkazMonsterModel() : base()
    {
        _currentLocation = InitialLocation;
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<EnterProtectionObjectivePower>(new ThrowingPlayerChoiceContext(), Creature, CalculatedLeftTurn, null, null);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != Creature.Side) return;
        // 回合末结算(中毒等)可能已经打死它；战斗正在收尾时也不再移动
        if (Creature.IsDead || Creature.CombatState?.IsLiveCombat() != true) return;

        ICombatState combatState = Creature.CombatState;

        // 本回合位移 = 当前移速（已含乘法减速），再经 delta 链修正
        // （传送/击退类一次性效果改这里的位移量）
        decimal delta = SarkazHook.ModifyMonsterMoveDelta(combatState.RunState, combatState, this, CurrentSpeed);
        CurrentLocation = _currentLocation + Math.Max(0, delta);

        await ApplyOrModifyEnterObjectivePower(choiceContext);

        if (_currentLocation >= Destination)
        {
            foreach (Player player in combatState.Players)
            {
                Creature target = player.Creature;
                if (target == null || target.IsDead) continue;
                await CreatureCmd.Damage(choiceContext, target, EscapeDamage, ValueProp.Unpowered, Creature);
            }
            await CreatureCmd.Escape(Creature);
            // 逃离完成后广播（此时已从敌人列表移除）；波次 encounter 靠它补下一波
            await SarkazHook.AfterMonsterEscaped(combatState.RunState, combatState, this);
        }
    }

    public async Task ApplyOrModifyEnterObjectivePower(PlayerChoiceContext choiceContext)
    {
        PowerModel timer = Creature.Powers.FirstOrDefault(p => p is EnterProtectionObjectivePower);
        if (timer != null && timer.Amount != CalculatedLeftTurn)
        {
            await PowerCmd.ModifyAmount(choiceContext, timer, CalculatedLeftTurn - timer.Amount, Creature, null, silent: true);
        }
    }

    // 默认动画机：Idle 循环 + Attack + Die(死亡触发 "Dead")。
    // 攻击招式用 WithAttackerAnim("Attack",...)，非攻击招式不触发动画。
    // 有额外动画需求的子类可再 override。
    public override CreatureAnimator GenerateAnimator(MegaSprite controller)
    {
        AnimState idle = new AnimState("Idle", isLooping: true);
        AnimState attack = new AnimState("Attack");
        AnimState die = new AnimState("Die");
        AnimState skillBegin = new AnimState("Skill_Begin");
        AnimState skillLoop = new AnimState("Skill_Loop");
        AnimState skillEnd = new AnimState("Skill_End");
        attack.NextState = idle;
        skillBegin.NextState = skillLoop;
        skillLoop.NextState = skillEnd;
        skillEnd.NextState = idle;
        CreatureAnimator animator = new CreatureAnimator(idle, controller);
        animator.AddAnyState("Attack", attack);
        animator.AddAnyState("Dead", die);
        animator.AddAnyState("Skill", skillBegin);
        return animator;
    }
}
