using System;
using System.Threading.Tasks;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

// 005 记功石柱：携带此遗愿进入新一层时，稀有度 +2。
public sealed class MemorialStones : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override Task AfterActEntered()
    {
        Rarity = Math.Min(4, Rarity + 2);
        return Task.CompletedTask;
    }
}