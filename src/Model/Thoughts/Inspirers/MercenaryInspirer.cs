using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

public sealed class MercenaryInspirer : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;
}
