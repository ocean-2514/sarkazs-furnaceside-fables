using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Commands;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Hooks;
using Skz.Ocean.Model;

namespace Skz.Ocean.Entities.Players;

public class PlayerExtraData
{
    private readonly List<ThoughtModel> _lingers = new List<ThoughtModel>();
    private readonly List<ThoughtModel> _inspirers = new List<ThoughtModel>();
    private readonly List<ThoughtModel> _plans = new List<ThoughtModel>();

    public IReadOnlyList<ThoughtModel> Lingers => _lingers;
    public IReadOnlyList<ThoughtModel> Inspirers => _inspirers;
    public IReadOnlyList<ThoughtModel> Plans => _plans;

    private ThoughtModel _thoughtUsed = null; 
    public ThoughtModel ThoughtUsed => _thoughtUsed;

    private int _toilLimit = 3;

    public int ToilLimit => _toilLimit;
    public int LucidLimit => _toilLimit;
    public int FracturedLimit => (int)(1.5 * _toilLimit);
    public event Action? ToilLimitChanged;
    public event Action? PlayerThoughtChanged;
    public event Action? ThoughtUsedChanged;
    public event Action? ShieldChanged;

    private int _shield = 10;

    public int Shield { 
        get => _shield;
        set
        {
            if (value < 0)
            {
                Log.Warn("Shield should must be greater than zero");
                return;
            }
            if (_shield == value) return;
            _shield = value;
            ShieldChanged?.Invoke();
        }
    } 

    private SarkazManagerModel? _sarkazManagerModel;

    public SarkazManagerModel SarkazManagerModel
    {
        get => _sarkazManagerModel;
        set
        {
            if (_sarkazManagerModel == null)
            {
                _sarkazManagerModel = value;
            }
            else
            {
                Log.Warn("Player already has a Sarkaz Manager!");
            }
        }
    }

    public int TotalToil
    {
        get
        {
            int tot = 0;
            tot += _inspirers.Sum(thought => thought.Toil);
            tot += _lingers.Sum(thought => thought.Toil);
            tot += _plans.Sum(thought => thought.Toil);
            return tot;
        }
    }

    public MindState MindState
    {
        get
        {
            int totalToil = TotalToil;
            if (totalToil <= LucidLimit) 
            {
                return MindState.Lucid;
            } 
            else if (totalToil <= FracturedLimit)
            {
                return MindState.Fractured;
            } 
            else
            {
                return MindState.Obstructed;
            }
        }
    }

    public IEnumerable<ThoughtModel> AllThoughts => 
        _lingers.Concat(_inspirers).Concat(_plans);

    public void AddThoughtInternal(ThoughtModel thought, Player player)
    {
        thought.AssertMutable();
        thought.Owner = player;
        switch (thought.ThoughtType)
        {
            case ThoughtType.Inspirer:
                _inspirers.Add(thought);
                break;
            case ThoughtType.Linger:
                _lingers.Add(thought);
                break;
            case ThoughtType.Plan:
                _plans.Add(thought);
                break;
        }
        thought.ToilChanged += OnThoughtChanged;
        thought.RarityChanged += OnThoughtChanged;
        PlayerThoughtChanged?.Invoke();
    }

    public void RemoveThoughtInternal(ThoughtModel thought)
    {
        switch (thought.ThoughtType)
        {
            case ThoughtType.Inspirer:
                if (!_inspirers.Contains(thought))
                {
                    throw new InvalidOperationException($"Player does not have thought {thought.Id}");
                }
                _inspirers.Remove(thought);
                break;
            case ThoughtType.Linger:
                if (!_lingers.Contains(thought))
                {
                    throw new InvalidOperationException($"Player does not have thought {thought.Id}");
                }
                _lingers.Remove(thought);
                break;
            case ThoughtType.Plan:
                if (!_plans.Contains(thought))
                {
                    throw new InvalidOperationException($"Player does not have thought {thought.Id}");
                }
                _plans.Remove(thought);
                break;
        }
        PlayerThoughtChanged?.Invoke();
    }

    public void RemovePlans(int count)
    {
        if (count > _plans.Count)
        {
            Log.Warn("Player don't have so many plans");
            return;
        }
        _plans.RemoveRange(0, count);
        PlayerThoughtChanged?.Invoke();
    }

    public async Task UseThoughtInternal(ThoughtModel thought)
    {
        if (thought.UseThoughtInternal())
        {
            _thoughtUsed = thought;
            RemoveThoughtInternal(thought);
            await SarkazHook.AfterThoughtUsed(thought.Owner.RunState, thought);
            ThoughtUsedChanged?.Invoke();
        }
    }

    public void ClearThoughtUsed()
    {
        _thoughtUsed = null;
        ThoughtUsedChanged?.Invoke();
    }

    // 读档恢复“已使用思绪”。它不在三大列表里(使用时已移出)，需单独恢复，
    // 这样存档发生在“用过灵感、本场战斗还没结束”时，读档后仍能在 AfterCombatEnd 正常结算。
    public void RestoreThoughtUsed(ThoughtModel thought)
    {
        _thoughtUsed = thought;
        ThoughtUsedChanged?.Invoke();
    }

    public void IncreaseToilLimit(int amount)
    {
        _toilLimit += amount;
        ToilLimitChanged?.Invoke();
    }

    // 读档时直接恢复持久化字段（护盾 / 苦役上限）。思绪列表由调用方逐个 AddThoughtInternal 恢复。
    public void RestorePersistentState(int shield, int toilLimit)
    {
        _toilLimit = toilLimit;
        ToilLimitChanged?.Invoke();
        Shield = shield; // 走属性，带非负校验 + ShieldChanged
    }

    private void OnThoughtChanged()
    {
        PlayerThoughtChanged?.Invoke();
    } 

    public ThoughtModel GetThoughtById(ModelId id)
    {
        return AllThoughts.FirstOrDefault((ThoughtModel thought) => thought.Id == id);
    }

    public void GainShield(int amount)
    {
        Shield = _shield + amount;
    }

    public void LoseShield(int amount)
    {
        Shield = _shield - amount;
    }
}