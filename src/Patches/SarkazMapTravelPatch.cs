using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Model;
using Skz.Ocean.Runs;

namespace Skz.Ocean.Patches;

// “横着走”：花费构想可以在当前这一排移动到一个兄弟节点（而非沿路线下行）。
//
// 原版 RecalculateTravelability 决定哪些节点可点：普通是当前节点的子节点(下一排相连)，
// 飞行靴的 ShouldAllowFreeTravel 钩子则是“下一排任意节点”。萨卡兹要的语义不同——同一排，
// 所以这里用 postfix 在原版算完之后，额外把当前排的其它节点设为 Travelable。
//
// 选点之后的链路(VoteForMapCoordAction → EnterMapCoord)没有相邻性校验，相邻性完全靠这里的
// Travelable 状态控制，所以改这里就能放行；构想的扣除在 SarkazManager.HandleHorizontalTravelCost
// (按“新旧节点同排”判定)。EnterMapCoord 用 coord.row+1 作为楼层，同排移动不增加楼层。
[HarmonyPatch(typeof(NMapScreen), "RecalculateTravelability")]
public static class SarkazMapTravelPatch
{
    public static void Postfix(NMapScreen __instance)
    {
        try
        {
            Traverse trav = Traverse.Create(__instance);
            RunState runState = trav.Field("_runState").GetValue<RunState>();
            if (runState == null) return;
            Player me = LocalContext.GetMe(runState);
            SarkazManagerModel manager = me?.GetSarkazManager();
            if (manager == null) return;
            ActMap map = trav.Field("_map").GetValue<ActMap>();
            var dict = trav.Field("_mapPointDictionary").GetValue<Dictionary<MapCoord, NMapPoint>>();
            NMapPoint _startingPointNode = trav.Field("_startingPointNode").GetValue<NMapPoint>();
            if (map == null || dict == null) return;

            if (!runState.VisitedMapCoords.Any())
            {
			    _startingPointNode.State = manager.CanWalkVertically ? MapPointState.Travelable : MapPointState.Untravelable;
            }

            IReadOnlyList<MapCoord> visited = runState.VisitedMapCoords;
            MapCoord current = visited[visited.Count - 1];

            if (dict.TryGetValue(current, out NMapPoint mapPoint))
            {
                IEnumerable<MapPoint> mapPoints = MapTravel.GetTravelablePointsFrom(runState, mapPoint.Point);
                foreach (MapPoint p in mapPoints)
                {
                    if (dict.TryGetValue(p.coord, out NMapPoint node))
                    {
                        node.State = manager.CanWalkVertically ? MapPointState.Travelable : MapPointState.Untravelable;
                    }
                }
            }

            if (me?.GetSarkazManager()?.CanWalkHorizontally != true) return;

            // 在 boss / 最后一排不提供横向移动（此时原版已 return，没有“下一排”可言）。
            if (current.row >= map.GetRowCount() - 1) return;

            // 只允许走到“相邻”的兄弟节点。原版地图相邻节点的列号差不一定是 1，
            // GetPointsInRow 按列号升序、跳过空列返回，所以相邻 = 该序列里 current 的前后一项。
            List<MapPoint> rowPoints = map.GetPointsInRow(current.row).ToList();
            int idx = rowPoints.FindIndex(p => p.coord == current);
            if (idx < 0) return;

            foreach (int neighbor in new[] { idx - 1, idx + 1 })
            {
                if (neighbor < 0 || neighbor >= rowPoints.Count) continue;
                if (!dict.TryGetValue(rowPoints[neighbor].coord, out NMapPoint node)) continue;
                if (node.State == MapPointState.Untravelable)
                {
                    node.State = MapPointState.Travelable;
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"[Sarkaz] 横向移动 travelability 计算失败: {e}");
        }
    }
}
