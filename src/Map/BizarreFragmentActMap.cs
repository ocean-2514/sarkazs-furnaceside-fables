using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;

namespace Skz.Ocean.Map;

// 诡谲断章的固定结构地图（GoldenPathActMap 同款写法）：
//   row 0: 火堆(RestSite) = StartingMapPoint —— 引擎规则：没有已走坐标时唯一可走的
//          就是起点节点本身(NMapScreen.RecalculateTravelability)，所以进层第一步就是进火堆。
//          非 Ancient 类型时 SetMap 会为它建普通节点(campfire 图标)，不会出现 Ancient 视觉。
//   row 1-3: 3×3 随机节点（池里没有 Unknown/Boss：避免随机事件与 Boss）
//   row 4: 出口节点（Unknown）——三条路汇聚；由 SarkazManagerModel 在本层内
//          强制 Event 房型 + ExitBizarreFragment 事件
//   BossMapPoint: 引擎多处假定非空，给一个不连通的占位点；NMapScreen 补丁隐藏它。
// 随机类型只在全新生成时掷（读档/回程走 SavedActMap 分支，不经 ModifyGeneratedMap），
// 所以布局天然随官方地图存档持久化。
public sealed class BizarreFragmentActMap : ActMap
{
    // 加权池：战斗占比高一点
    private static readonly MapPointType[] _randomPool =
    {
        MapPointType.Monster, MapPointType.Monster,
        MapPointType.Shop, MapPointType.Treasure,
        MapPointType.RestSite, MapPointType.Elite,
    };

    public override MapPoint BossMapPoint { get; }

    public override MapPoint StartingMapPoint { get; }

    protected override MapPoint?[,] Grid { get; }

    public BizarreFragmentActMap(IRunState runState)
    {
        Rng rng = (runState as RunState)?.Rng.Niche;

        Grid = new MapPoint[7, 5];

        // row 0: 火堆 = 起点（不放进 Grid：SetMap 会为 StartingMapPoint 单独建节点，
        // 若同时在 Grid 里会画出两个重叠节点）
        MapPoint rest = new MapPoint(3, 0) { PointType = MapPointType.RestSite };
        StartingMapPoint = rest;

        // row 1-3: 3×3 随机节点；火堆连向第一行，之后各列直连成三条路
        for (int r = 1; r <= 3; r++)
        {
            for (int c = 1; c <= 5; c += 2)
            {
                MapPoint p = new MapPoint(c, r)
                {
                    PointType = _randomPool[rng?.NextInt(_randomPool.Length) ?? 0],
                };
                Grid[c, r] = p;
                if (r == 1)
                {
                    rest.AddChildPoint(p);
                    startMapPoints.Add(p); // 仅供生成期工具使用，不影响行走
                }
                else
                {
                    Grid[c, r - 1].AddChildPoint(p);
                }
            }
        }

        // row 4: 出口，三条路汇聚
        MapPoint exit = new MapPoint(3, 4)
        {
            PointType = MapPointType.Elite,
            CanBeModified = false, // 不许被节点刷新等机制改类型
        };
        Grid[3, 4] = exit;
        for (int c = 1; c <= 5; c += 2)
        {
            Grid[c, 3].AddChildPoint(exit);
        }

        // 不连通的占位 Boss 点（行号在网格之外，GoldenPath 同款），由补丁隐藏。
        BossMapPoint = new MapPoint(3, GetRowCount()) { PointType = MapPointType.Boss };
    }

    public static bool IsExitMapPoint(MapPoint mapPoint)
    {
        return mapPoint.coord.col == 3 && mapPoint.coord.row == 4;
    }
}
