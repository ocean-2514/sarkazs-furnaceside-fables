using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

public sealed class Lost : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 4;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;
}
