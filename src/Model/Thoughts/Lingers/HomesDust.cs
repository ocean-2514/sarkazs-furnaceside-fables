using System.Threading.Tasks;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

// 007 旧乡晶尘：失去后进入新一层会再次回到手上。
public sealed class HomesDust : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    public override Task AfterRemoved()
    {
        // 登记到 manager 的待返还清单；进入新一层时由 manager 重新发放(并持久化)。
        Owner.GetSarkazManager()?.RegisterReturn(Id);
        return Task.CompletedTask;
    }
}