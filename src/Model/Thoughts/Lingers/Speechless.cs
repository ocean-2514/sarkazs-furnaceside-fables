using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class Speechless : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Event;
}
