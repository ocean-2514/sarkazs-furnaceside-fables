using System.Collections.Generic;
using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Map;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Acts;
using MegaCrit.Sts2.Core.Models.Encounters;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.Timeline.Epochs;
using MegaCrit.Sts2.Core.Unlocks;
using Skz.Ocean.Saves;

namespace Skz.Ocean.Model.Acts;

public sealed class BizarreFragment : ActModel
{
    // ────────────────────────────
    //  注册属性（新版 ActModel 抽象成员）
    // ────────────────────────────
    // 负数 = 永不出现在正常 act 生成序列里；本层只由 MapDetour 动态追加进入。
    public override int Index => -1;

    public override bool IsDefault => false;

    public override bool IsUnlocked(UnlockState unlockState)
    {
        return true;
    }

    // ────────────────────────────
    //  地图颜色
    // ────────────────────────────
    public override Color MapTraveledColor => new Color("1A3A2A");   // 深绿
    public override Color MapUntraveledColor => new Color("4A7C59"); // 灰绿
    public override Color MapBgColor => new Color("87B38D");         // 淡鼠尾草绿

    // ────────────────────────────
    //  音频/宝箱资源：跟随"从哪个原版 act 进来"（FMOD 事件按 act 编号命名，
    //  没法拿名字拼路径，直接委托来源 act 的属性最稳）；解析不到时兜底 Act 1。
    // ────────────────────────────
    private static ActModel SourceAct => Skz.Ocean.Saves.MapDetour.GetSourceAct();

    public override string ChestOpenSfx =>
        SourceAct?.ChestOpenSfx ?? "event:/sfx/ui/treasure/treasure_act1";
    public override string[] BgMusicOptions =>
        SourceAct?.BgMusicOptions ?? new[] { "event:/music/act1_a1_v1", "event:/music/act1_a2_v2" };
    public override string[] MusicBankPaths =>
        SourceAct?.MusicBankPaths ?? new[] { "res://banks/desktop/act1_a1.bank", "res://banks/desktop/act1_a2.bank" };
    public override string AmbientSfx =>
        SourceAct?.AmbientSfx ?? "event:/sfx/ambience/act1_ambience";

    // ────────────────────────────
    //  宝箱 Spine 资源：同样跟随来源 act
    // ────────────────────────────
    public override string ChestSpineResourcePath =>
        SourceAct?.ChestSpineResourcePath ?? "res://animations/backgrounds/treasure_room/chest_room_act_1_skel_data.tres";
    public override string ChestSpineSkinNameNormal => SourceAct?.ChestSpineSkinNameNormal ?? "act1";
    public override string ChestSpineSkinNameStroke => SourceAct?.ChestSpineSkinNameStroke ?? "act1_stroke";

    // ────────────────────────────
    //  房间数量
    // ────────────────────────────
    protected override int NumberOfWeakEncounters => 3;
    protected override int BaseNumberOfRooms => 16;

    // ────────────────────────────
    //  Boss 发现顺序（教程用）
    // ────────────────────────────
    public override IEnumerable<EncounterModel> BossDiscoveryOrder => new EncounterModel[]
    {
        ModelDb.Encounter<VantomBoss>(),
        ModelDb.Encounter<CeremonialBeastBoss>(),
        ModelDb.Encounter<TheKinBoss>(),
    };

    // ────────────────────────────
    //  Ancient 事件
    // ────────────────────────────
    public override IEnumerable<AncientEventModel> AllAncients => new AncientEventModel[]
    {
        ModelDb.AncientEvent<Neow>(),
    };

    // ────────────────────────────
    //  普通事件（混合 Act1 + 共享事件）
    // ────────────────────────────
    public override IEnumerable<EventModel> AllEvents => new EventModel[]
    {
        ModelDb.Event<Skz.Ocean.Model.Events.ExitBizarreFragment>()
    };

    // ────────────────────────────
    //  全部遭遇战列表
    // ────────────────────────────
    public override IEnumerable<EncounterModel> GenerateAllEncounters()
    {
        return MapDetour.GetSourceAct().GenerateAllEncounters();
    }

    // ────────────────────────────
    //  解锁过滤
    // ────────────────────────────
    // 无条件返回：GenerateRooms 会对结果 rng.NextItem，空列表会抛异常。
    // 本层地图没有 Ancient 节点，这个 Ancient 只用于满足 RoomSet/AssetPaths，永不进入。
    public override IEnumerable<AncientEventModel> GetUnlockedAncients(UnlockState unlockState)
    {
        return AllAncients;
    }

    // ────────────────────────────
    //  教程模式调整（不做特殊处理）
    // ────────────────────────────
    protected override void ApplyActDiscoveryOrderModifications(UnlockState unlockState)
    {
    }

    // ────────────────────────────
    //  地图节点分布（篝火 6~8 个，Unknown 用标准分布）
    // ────────────────────────────
    public override MapPointTypeCounts GetMapPointTypes(Rng mapRng)
    {
        int restCount = mapRng.NextGaussianInt(7, 1, 6, 8);
        int unknownCount = MapPointTypeCounts.StandardRandomUnknownCount(mapRng);
        return new MapPointTypeCounts(unknownCount, restCount);
    }
}