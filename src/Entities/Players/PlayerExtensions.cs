using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using Skz.Ocean.Model;

namespace Skz.Ocean.Entities.Players;

public static class PlayerExtension
{
    private static ConditionalWeakTable<Player, PlayerExtraData> _dataTable = new ConditionalWeakTable<Player, PlayerExtraData>();

    public static PlayerExtraData GetExtraData(this Player player)
    {
        return _dataTable.GetOrCreateValue(player);
    }
    
    public static void AddThoughtInternal(this Player player, ThoughtModel thought)
    {
        player.GetExtraData().AddThoughtInternal(thought, player);
    }

    public static void RemoveThoughtInternal(this Player player, ThoughtModel thought)
    {
        player.GetExtraData().RemoveThoughtInternal(thought);
    }

    public static IReadOnlyList<ThoughtModel> GetLingers(this Player player)
    {
        return player.GetExtraData().Lingers;
    }
    public static IReadOnlyList<ThoughtModel> GetPlans(this Player player)
    {
        return player.GetExtraData().Plans;
    }
    public static IReadOnlyList<ThoughtModel> GetInspirers(this Player player)
    {
        return player.GetExtraData().Inspirers;
    }

    public static IEnumerable<ThoughtModel> GetAllThoughts(this Player player)
    {
        PlayerExtraData data = player.GetExtraData();
        return data.Lingers.Concat(data.Inspirers).Concat(data.Plans);
    }

    public static void ClearThoughtUsed(this Player player)
    {
        player.GetExtraData().ClearThoughtUsed();
    }

    public static async Task UseThought(this Player player, ThoughtModel thought)
    {
        await player.GetExtraData().UseThoughtInternal(thought);
    }

}