using MegaCrit.Sts2.Core.Entities.Players;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 031 启蒙：使用后下次战斗每回合多抽一张牌。
public sealed class Initiate : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 2;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (Status != ThoughtStatus.Used || player != Owner) return count;
        return count + 1;
    }
}