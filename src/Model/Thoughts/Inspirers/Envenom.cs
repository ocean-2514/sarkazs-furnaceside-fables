using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

public sealed class Envenom : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;
}
