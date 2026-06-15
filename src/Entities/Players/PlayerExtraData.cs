using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
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
    }

    public async Task UseThoughtInternal(ThoughtModel thought)
    {
        if (thought.UseThoughtInternal())
        {
            if (_thoughtUsed != null)
            {
                await ThoughtCmd.Remove(_thoughtUsed);
            }
            _thoughtUsed = thought;
            await SarkazHook.AfterThoughtUsed(thought.Owner.RunState, thought);
        }
    }

    public void ClearThoughtUsed()
    {
        _thoughtUsed = null;
    }
}