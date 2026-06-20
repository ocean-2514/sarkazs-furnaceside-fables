using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

public sealed class CivilWar : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 1;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;
}
