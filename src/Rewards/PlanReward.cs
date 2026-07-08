using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Thoughts.Plans;

namespace Skz.Ocean.Rewards;

public sealed class PlanReward : ThoughtReward
{
    public PlanReward(Player player) : 
        base(player) {}

    public override void Populate()
    {
        _thought = SarkazHelper.ModelDbGet<Plan>().ToMutable();
    }
}