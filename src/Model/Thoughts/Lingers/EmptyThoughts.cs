using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class EmptyThoughts : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;
}
