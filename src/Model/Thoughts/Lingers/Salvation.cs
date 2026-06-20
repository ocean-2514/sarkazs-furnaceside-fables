using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class Salvation : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Event;
}
