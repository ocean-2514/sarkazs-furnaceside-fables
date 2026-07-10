using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Random;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class WildGrassByTheDoor : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 4;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override Task AfterActEntered()
    {
        if (Owner.GetSarkazManager().PreviousActWasFragment) return Task.CompletedTask;
        Rng rng = Owner.PlayerRng.Rewards;
        int rarity = rng.NextInt(1, 5);
        while (rarity == Rarity)
        {
            rarity = rng.NextInt(1, 5);
        }
        Rarity = rarity;
        return Task.CompletedTask;
    }

}
