using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

public sealed class RiseAndFall : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;
}
