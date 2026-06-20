using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

public sealed class StoredInstruments : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;
}
