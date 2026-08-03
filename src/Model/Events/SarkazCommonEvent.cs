using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Ascension;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Commands;
using Skz.Ocean.Helper;
using Skz.Ocean.Model.Relics;
using Skz.Ocean.Model.Thoughts.Plans;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Model.Events;

public static class SarkazCommonEvent
{
    private static float EnterFragmentChance => AscensionHelper.GetValueIfAscension(AscensionLevel.Scarcity, 0.05f, 0.08f);
    private static float GainKingRelicChance => 1.0f;

    // 两套收藏品：低血线给国王套，满血给魔王套（判定标准分别对齐 KingsCrown / KingOfSarkazsVessel）
    private static readonly Type[] KingSet =
        [typeof(KingsCrown), typeof(KingsNewLance), typeof(KingsArmor), typeof(KingsLegacy)];
    private static readonly Type[] SarkazKingSet =
        [typeof(SarkazKingsRegalRest), typeof(SarkazKingsTornBanner), typeof(KingOfSarkazsVessel)];

    public static EventOption CreateCommonOption(EventModel eventModel)
    {
        return new EventOption(
            eventModel, 
            () => GetHelpInternal(eventModel), 
            "SARKAZ_COMMON_EVENT.pages.INITIAL.options.GET_HELP"
        );
    }

    public static Task GetHelpInternal(EventModel eventModel)
    {
        Player player = eventModel.Owner;
        Traverse traverse = Traverse.Create(eventModel);
        List<EventOption> options = new List<EventOption>([
            new EventOption(eventModel, () => GetRelicInternal(eventModel), "SARKAZ_COMMON_EVENT.pages.options.GET_RELIC"),
            GeneratePlanOption(eventModel)
        ]);
        if (player.PlayerRng.Rewards.NextFloat() < EnterFragmentChance)
        {
            options.Add(new EventOption(eventModel, () => EnterInternal(eventModel), "SARKAZ_COMMON_EVENT.pages.options.ENTER_FRAGMENT"));
        }
        if (IsLowHp(player) && !OwnAllRelic(eventModel, KingSet))
        {
            options.Add(new EventOption(eventModel, () => GetSetRelicInternal(eventModel, KingSet), "SARKAZ_COMMON_EVENT.pages.options.GET_KING_RELIC"));
        }
        else if (player.Creature.CurrentHp >= player.Creature.MaxHp && !OwnAllRelic(eventModel, SarkazKingSet))
        {
            options.Add(new EventOption(eventModel, () => GetSetRelicInternal(eventModel, SarkazKingSet), "SARKAZ_COMMON_EVENT.pages.options.GET_SARKAZ_KING_RELIC"));
        }
        traverse.Method("SetEventState", eventModel.InitialDescription, options).GetValue();

        return Task.CompletedTask;
    }

    private static EventOption GeneratePlanOption(EventModel eventModel)
    {
        Player player = eventModel.Owner;
        if (player.Gold >= 100)
        {
            return new EventOption(eventModel, () => GetPlanInternal(eventModel), "SARKAZ_COMMON_EVENT.pages.options.GET_PLAN");
        }
        else
        {
            return new EventOption(eventModel, null, "SARKAZ_COMMON_EVENT.pages.options.GET_PLAN");
        }
    }

    private static Task EnterInternal(EventModel eventModel)
    {
        Player player = eventModel.Owner;
        Traverse.Create(eventModel).Method("SetEventFinished", new LocString("events", "SARKAZ_COMMON_EVENT.pages.EXIT.description")).GetValue();


        // 不能在选项 task 里 await 切层：EnterAct 会退出本事件房间，而
        // EventRoom.Exit → EventSynchronizer.AwaitPendingOptionTasks 会等本选项 task
        // 完成 → 自己等自己死锁。拆成 fire-and-forget，选项 task 先结束，
        // 切层（压书签 → 追加子层 act → EnterAct）在下一个异步节拍执行。
        TaskHelper.RunSafely(EnterFragmentDetached(player.RunState as RunState, player));
        return Task.CompletedTask;
    }

    private static async Task EnterFragmentDetached(RunState state, Player player)
    {
        await Task.Yield(); // 等本次选项调用栈完全退出再开始切层
        await MapDetour.EnterFragment(state, player);
    }

    private static async Task GetPlanInternal(EventModel eventModel)
    {
        Player player = eventModel.Owner;
        await PlayerCmd.LoseGold(50m, player);
        await ThoughtCmd.Obtain<Plan>(player);
        Traverse.Create(eventModel).Method("SetEventFinished", new LocString("events", "SARKAZ_COMMON_EVENT.pages.EXIT.description")).GetValue();
    }
    
    private static async Task GetRelicInternal(EventModel eventModel)
    {
        Player player = eventModel.Owner;
        await RelicCmd.Obtain(RelicFactory.PullNextRelicFromFront(player, RollRarity(player.PlayerRng.Rewards)).ToMutable(), player);
        Traverse.Create(eventModel).Method("SetEventFinished", new LocString("events", "SARKAZ_COMMON_EVENT.pages.EXIT.description")).GetValue();
    }

    private static bool IsLowHp(Player player) =>
        player.Creature.CurrentHp <= 10
        || player.Creature.CurrentHp < player.Creature.MaxHp * 10 / 100;

    // 概率成功时从对应套装里随机给一件未持有的；套装齐了或掷骰失败都只结束事件
    private static async Task GetSetRelicInternal(EventModel eventModel, Type[] set)
    {
        Player player = eventModel.Owner;
        Rng rng = player.PlayerRng.Rewards;
        if (rng.NextFloat() < GainKingRelicChance)
        {
            List<Type> candidates = set
                .Where(t => !player.Relics.Any(r => r.GetType() == t))
                .ToList();
            if (candidates.Count > 0)
            {
                RelicModel relic = (RelicModel)SarkazHelper.ModelDbGet(candidates[rng.NextInt(candidates.Count)]);
                await RelicCmd.Obtain(relic.ToMutable(), player);
            }
        }
        Traverse.Create(eventModel).Method("SetEventFinished", new LocString("events", "SARKAZ_COMMON_EVENT.pages.EXIT.description")).GetValue();
    }

    private static RelicRarity RollRarity(Rng rng)
    {
        if (rng.NextFloat() < 0.75) return RelicRarity.Common;
        else return RelicRarity.Uncommon;
    }

    private static bool OwnAllRelic(EventModel eventModel, Type[] set)
    {
        List<Type> candidates = set.Where(t => !eventModel.Owner.Relics.Any(r => r.GetType() == t)).ToList();
        return candidates.Count == 0;
    }
}