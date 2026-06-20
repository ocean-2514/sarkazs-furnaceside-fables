using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Plans;

public sealed class Plan : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Plan;

    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Rare;
}