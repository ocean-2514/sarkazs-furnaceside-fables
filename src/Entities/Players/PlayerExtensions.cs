using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Events;
using Skz.Ocean.Model;

namespace Skz.Ocean.Entities.Players;

public static class PlayerExtensions
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

    public static int GetToilLimit(this Player player) 
    {
        return player.GetExtraData().ToilLimit;
    }
    public static int GetLucidLimit(this Player player) 
    {
        return player.GetExtraData().ToilLimit;
    }
    public static int GetFracturedLimit(this Player player) 
    {
        return (int)(1.5 * player.GetExtraData().ToilLimit);
    }

    public static int GetTotalToil(this Player player)
    {
        return player.GetExtraData().TotalToil;
    }

    public static void IncreaseToilLimit(this Player player, int amount)
    {
        player.GetExtraData().IncreaseToilLimit(amount);
    }

    public static MindState GetMindState(this Player player)
    {
        return player.GetExtraData().MindState;
    }

    public static ThoughtModel GetThoughtUsed(this Player player)
    {
        return player.GetExtraData().ThoughtUsed;
    }

    public static ThoughtModel GetThoughtById(this Player player, ModelId id)
    {
        return player.GetExtraData().GetThoughtById(id);
    }

    public static int GetPlanCount(this Player player)
    {
        return player.GetExtraData().Plans.Count;
    }

    public static void RemovePlans(this Player player, int count)
    {
        player.GetExtraData().RemovePlans(count);
    }

    public static SarkazManagerModel GetSarkazManager(this Player player)
    {
        return player.GetExtraData().SarkazManagerModel;
    }

    public static SarkazCombatManagerModel GetCombatManager(this Player player)
    {
        return player.GetExtraData().SarkazCombatManagerModel;
    }

    // 幂等地给玩家挂上 SarkazManagerModel。
    // 新开局(CreateForNewRun)与读档(FromSerializable)都会调用。
    public static void EnsureSarkazManager(this Player player)
    {
        if (player.GetExtraData().SarkazManagerModel == null)
        {
            SarkazManagerModel manager = Helper.SarkazHelper.ModelDbGet<SarkazManagerModel>().ToMutable();
            manager.Owner = player;
            player.GetExtraData().SarkazManagerModel = manager;
        }
        if (player.GetExtraData().SarkazCombatManagerModel == null)
        {
            SarkazCombatManagerModel combatManager = Helper.SarkazHelper.ModelDbGet<SarkazCombatManagerModel>().ToMutable();
            combatManager.Owner = player;
            player.GetExtraData().SarkazCombatManagerModel = combatManager;
        }
    }

    public static void GainShield(this Player player, int amount)
    {
        player.GetExtraData().GainShield(amount);
    }
    
    public static void LoseShield(this Player player, int amount)
    {
        player.GetExtraData().LoseShield(amount);
    }

    public static int GetShield(this Player player)
    {
        return player.GetExtraData().Shield;
    }
}