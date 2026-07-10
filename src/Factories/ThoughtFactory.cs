using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;
using Skz.Ocean.Model;

namespace Skz.Ocean.Factories;

// 仿原版 RelicFactory：决定战斗奖励里给哪个思绪。
public static class ThoughtFactory
{
    // 按房型掷稀有度(Common/Uncommon/Rare)：
    // 普通 50/33/17（与原版遗物一致），精英 20/50/30，Boss 0/50/50。
    public static ThoughtRarity RollThoughtRarity(Rng rng, RoomType roomType = RoomType.Monster)
    {
        float r = rng.NextFloat();
        switch (roomType)
        {
            case RoomType.Elite:
                if (r < 0.20f) return ThoughtRarity.Common;
                if (r < 0.70f) return ThoughtRarity.Uncommon;
                return ThoughtRarity.Rare;
            case RoomType.Boss:
                if (r < 0.50f) return ThoughtRarity.Uncommon;
                return ThoughtRarity.Rare;
            default:
                if (r < 0.50f) return ThoughtRarity.Common;
                if (r < 0.83f) return ThoughtRarity.Uncommon;
                return ThoughtRarity.Rare;
        }
    }

    // 战斗奖励池：Common/Uncommon/Rare，且排除构想(Plan)。
    // 先 50/50 掷类型(灵感/遗愿)，再按房型掷稀有度；
    // 该类型没有该稀有度时退回该类型全稀有度（保住 50/50 类型分布），类型池也空才退回整个奖励池。
    // 返回可变克隆；池空返回 null。
    public static ThoughtModel CreateRandomThought(Player player, Rng rng, RoomType roomType = RoomType.Monster)
    {
        ThoughtRarity rarity = RollThoughtRarity(rng, roomType);
        ThoughtType type = rng.NextFloat() < 0.5f ? ThoughtType.Inspirer : ThoughtType.Linger;

        List<ThoughtModel> rewardable = SarkazHelper.GetAllThoughts()
            .Where(t => t.ThoughtType != ThoughtType.Plan && IsRewardRarity(t.ThoughtRarity))
            .ToList();
        if (rewardable.Count == 0) return null;

        List<ThoughtModel> typed = rewardable.Where(t => t.ThoughtType == type).ToList();
        List<ThoughtModel> pool = typed.Where(t => t.ThoughtRarity == rarity).ToList();
        if (pool.Count == 0) pool = typed;
        if (pool.Count == 0) pool = rewardable;

        return pool[rng.NextInt(pool.Count)].ToMutable();
    }

    public static ThoughtModel CreateRandomInspirer(Player player, Rng rng)
    {
        List<ThoughtModel> pool = SarkazHelper.GetAllThoughts()
            .Where(t => t.ThoughtType == ThoughtType.Inspirer && IsRewardRarity(t.ThoughtRarity))
            .ToList();
        if (pool.Count == 0) return null;
        return pool[rng.NextInt(pool.Count)].ToMutable();
    }

    public static ThoughtModel CreateRandomLinger(Player player, Rng rng)
    {
        List<ThoughtModel> pool = SarkazHelper.GetAllThoughts()
            .Where(t => t.ThoughtType == ThoughtType.Linger && IsRewardRarity(t.ThoughtRarity))
            .ToList();
        if (pool.Count == 0) return null;
        return pool[rng.NextInt(pool.Count)].ToMutable();
    }

    private static bool IsRewardRarity(ThoughtRarity r) =>
        r == ThoughtRarity.Common || r == ThoughtRarity.Uncommon || r == ThoughtRarity.Rare;
}