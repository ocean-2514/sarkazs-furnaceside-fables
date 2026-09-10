using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using Skz.Ocean.Entities.Ascension;
using Skz.Ocean.Helper;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Encounters.Base;

// 波次型战斗基类：开场只出第 0 波，当前波的主要敌人全部死亡/逃离后补下一波。
// 靠 ShouldStopCombatFromEnding 否决"敌人清空即胜利"（原版 StockPower 同款机制，
// 但票投在 encounter 上，不依赖任何存活的 Power 宿主）。
// 进入钩子系统的路径：ModInitializer.CollectSarkazCombatModels 把当前 encounter
// 加进 ModHelper 战斗订阅（收原版钩子），IterateSarkazHookListeners 里也会带上
// （收 SarkazHook.AfterMonsterEscaped）。
// 战斗中途不存档（SL 整场重打），波次进度无需持久化。
public abstract class SarkazEncounterModel : EncounterModel, ISarkazModel
{
    // EncounterModel 默认 false；需要收 AfterDeath / ShouldStopCombatFromEnding
    public override bool ShouldReceiveCombatHooks => true;

    // 波次条目：怪物工厂（出怪时调用，返回 canonical 或 mutable 均可）
    // + 指定槽位（null = 自动取当前空槽）。
    public readonly record struct WaveEntry(Func<MonsterModel> Factory, string? Slot = null);

    // 子类提供波次定义。外层 = 波（按顺序出），内层 = 该波的怪。第 0 波为开场怪。
    // 每场战斗会在 GenerateMonsters 时调用一次（mutable 实例上）。
    protected abstract IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves();

    // 必须为 true，否则 NCombatRoom 不会加载 scenes/encounters/mock_spines_of_epoch.tscn，
    // EncounterSlots 为 null，带槽位的怪物入场时会抛异常。
    public override bool HasScene => true;

    // 按屏幕从左到右排列：CombatState 用它给敌人排序（IndexOf 取顺序）。
    public override IReadOnlyList<string> Slots => ["first", "second", "third", "forth", "fifth", "sixth"];

    private IReadOnlyList<IReadOnlyList<WaveEntry>> _waves;
    private int _nextWaveIndex;
    private bool _advancing;

    public bool HasPendingWaves => _waves != null && _nextWaveIndex < _waves.Count;

    public int RemainingWaves => _waves == null ? 0 : Math.Max(0, _waves.Count - _nextWaveIndex);

    protected sealed override IReadOnlyList<(MonsterModel, string?)> GenerateMonsters()
    {
        _waves = GenerateWaves();
        _nextWaveIndex = 1;
        if (_waves == null || _waves.Count == 0)
        {
            return Array.Empty<(MonsterModel, string?)>();
        }
        // 用了具名槽位就必须有场景，否则 NCombatRoom.EncounterSlots 为 null，
        // 怪物入场时才炸（且报错指向渲染层，不好查）。这里提前给出明确报错。
        if (!HasScene && _waves.Any(w => w.Any(e => e.Slot != null)))
        {
            throw new InvalidOperationException(
                $"{GetType().Name} 的波次里指定了槽位，但 HasScene 为 false。" +
                $"请重写 HasScene => true 并提供 scenes/encounters/{Id.Entry.ToLowerInvariant()}.tscn，" +
                "同时重写 Slots 列出全部槽位名。");
        }
        return _waves[0].Select(e => (Materialize(e.Factory), e.Slot)).ToList();
    }

    private static MonsterModel Materialize(Func<MonsterModel> factory)
    {
        MonsterModel monster = factory();
        return monster.IsMutable ? monster : monster.ToMutable();
    }

    // 还有波未出时否决战斗结束（AbstractModel 虚方法，Hook.ShouldStopCombatFromEnding 收集投票）
    public override bool ShouldStopCombatFromEnding()
    {
        return HasPendingWaves;
    }

    // 敌人死亡 → 尝试推进。新波先隐身、等死亡动画播完再显形（StockPower 同款节奏）。
    public override async Task AfterDeath(PlayerChoiceContext choiceContext, Creature target, bool wasRemovalPrevented, float deathAnimLength)
    {
        if (wasRemovalPrevented || target == null || target.Side != CombatSide.Enemy) return;
        await TryAdvanceWave(target.CombatState, deathAnimLength);
    }

    // 敌人进蓝门逃离 → 尝试推进（SarkazMonsterModel 逃离后经 SarkazHook 通知，无动画等待）
    public virtual async Task AfterMonsterEscaped(IRunState runState, ICombatState combatState, SarkazMonsterModel monster)
    {
        await TryAdvanceWave(combatState, 0f);
    }

    private async Task TryAdvanceWave(ICombatState combatState, float revealDelay)
    {
        if (_advancing || !HasPendingWaves) return;
        if (combatState == null || !combatState.IsLiveCombat()) return;
        // 当前波还有活着的主要敌人（年代之刺等 secondary 不算）就不推进
        if (combatState.Enemies.Any(c => c.IsAlive && c.IsPrimaryEnemy)) return;

        _advancing = true;
        try
        {
            IReadOnlyList<WaveEntry> wave = _waves[_nextWaveIndex];
            _nextWaveIndex++;
            foreach (WaveEntry entry in wave)
            {
                MonsterModel monster = Materialize(entry.Factory);
                string slot = entry.Slot;
                if (slot == null)
                {
                    string next = GetNextSlot(combatState);
                    slot = string.IsNullOrEmpty(next) ? null : next;
                }
                Creature creature = await CreatureCmd.Add(monster, combatState, CombatSide.Enemy, slot);
                if (!SarkazAscensionHelper.HasAscension(SarkazAscensionLevel.UnstunnedWaves))
                {
                    await CreatureCmd.Stun(creature);
                }
                if (revealDelay > 0f && creature != null)
                {
                    creature.SetNodeVisible(visible: false);
                    TaskHelper.RunSafely(RevealAfterDelay(creature, revealDelay));
                }
            }
        }
        finally
        {
            _advancing = false;
        }
    }

    private static async Task RevealAfterDelay(Creature creature, float delay)
    {
        await Cmd.CustomScaledWait(delay, delay);
        creature.SetNodeVisible(visible: true);
    }
}
