using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Plans;

public sealed class Plan : ThoughtModel
{
    public override int Rarity => 3;
    public override int Toil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Plan;
}