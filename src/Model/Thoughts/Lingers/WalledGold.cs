using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class WalledGold : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;
}
