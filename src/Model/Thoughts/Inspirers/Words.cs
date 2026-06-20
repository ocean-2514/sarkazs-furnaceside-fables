using System;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

public sealed class Words : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.None;

    public override bool CanBeUpgraded => true;
    public override Type UpgradedType => typeof(Assembly);


}
