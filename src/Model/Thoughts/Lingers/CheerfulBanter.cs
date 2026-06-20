using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class CheerfulBanter : ThoughtModel
{
    protected override int InitialRarity => 4;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;
}
