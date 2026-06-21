using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Random;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;
using Skz.Ocean.Model;

namespace Skz.Ocean.Factories;

// 仿原版 RelicFactory：决定战斗奖励里给哪个思绪。
public static class ThoughtFactory
{
    // 按权重掷出稀有度（与原版遗物一致：普通 50% / 罕见 33% / 稀有 17%）。
    public static ThoughtRarity RollThoughtRarity(Rng rng)
    {
        float r = rng.NextFloat();
        if (r < 0.50f) return ThoughtRarity.Common;
        if (r < 0.83f) return ThoughtRarity.Uncommon;
        return ThoughtRarity.Rare;
    }

    // 战斗奖励池：Common/Uncommon/Rare，且排除构想(Plan)。
    // 先按掷出的稀有度选；该稀有度没有候选时退回整个奖励池。返回可变克隆；池空返回 null。
    public static ThoughtModel? CreateRandomThought(Player player, Rng rng)
    {
        ThoughtRarity rarity = RollThoughtRarity(rng);

        List<ThoughtModel> rewardable = SarkazHelper.GetAllThoughts()
            .Where(t => t.ThoughtType != ThoughtType.Plan
                     && (t.ThoughtRarity == ThoughtRarity.Common
                      || t.ThoughtRarity == ThoughtRarity.Uncommon
                      || t.ThoughtRarity == ThoughtRarity.Rare))
            .ToList();
        if (rewardable.Count == 0) return null;

        List<ThoughtModel> pool = rewardable.Where(t => t.ThoughtRarity == rarity).ToList();
        if (pool.Count == 0) pool = rewardable;

        return pool[rng.NextInt(pool.Count)].ToMutable();
    }
}
