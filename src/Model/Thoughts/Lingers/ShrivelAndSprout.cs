using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class ShrivelAndSprout : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;
}
