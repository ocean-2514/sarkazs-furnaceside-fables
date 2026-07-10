using System;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Logging;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class DaybreakSun : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override Task AfterActEntered()
    {
        if (Owner.GetSarkazManager().PreviousActWasFragment) return Task.CompletedTask;
        Rarity = Math.Min(4, Rarity + 1);
        return Task.CompletedTask;
    }

}
