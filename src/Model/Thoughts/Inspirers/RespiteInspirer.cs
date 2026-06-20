using System;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

public sealed class RespiteInspirer : ThoughtModel
{
    protected override int InitialRarity => 1;
    protected override int InitialToil => 1;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override bool CanBeUpgraded => true;
    public override Type UpgradedType => typeof(Slumber);


}
