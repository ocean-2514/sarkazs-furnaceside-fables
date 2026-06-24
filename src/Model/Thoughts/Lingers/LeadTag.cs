using System.Threading.Tasks;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

// 009 灌铅胸牌：失去后金币 +30。
public sealed class LeadTag : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Event;

    public override Task AfterRemoved()
    {
        Owner.Gold += 30;
        return Task.CompletedTask;
    }
}