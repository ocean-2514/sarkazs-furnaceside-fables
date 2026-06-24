using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 047 爆破：使用后下次战斗年代之刺最大生命值 -50%。
// 暂不实现：相关敌人(年代之刺)尚未制作，先排除出奖励池(None)。
public sealed class Demolition : ThoughtModel
{
    protected override int InitialRarity => 2;
    protected override int InitialToil => 3;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.None;
}