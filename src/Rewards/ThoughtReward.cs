using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.TestSupport;
using Skz.Ocean.Commands;
using Skz.Ocean.Factories;
using Skz.Ocean.Model;

namespace Skz.Ocean.Rewards;

// 战斗后的“思绪奖励”——独立于卡牌/遗物奖励的额外奖励按钮。
// 由 SarkazManagerModel.TryModifyRewards 注入到结算奖励列表(纯新增，不改动其它奖励)。
// 不走 CombatRoom.ExtraRewards：那条路会被序列化，而 Reward.FromSerializable 对未知类型会抛异常；
// 走生成/ModifyRewards 路径则永不序列化，读档时由 hook 重新生成（与原版遗物奖励一致）。
public class ThoughtReward : Reward
{
    protected ThoughtModel? _thought;

    // 掷稀有度用：普通/精英/Boss 掉率不同（见 ThoughtFactory.RollThoughtRarity）。
    private readonly RoomType _roomType;

    public ThoughtReward(Player player, RoomType roomType = RoomType.Monster) : base(player)
    {
        _roomType = roomType;
    }

    // 仅序列化路径会用到；本奖励永不序列化，取一个现有枚举值占位即可。
    protected override RewardType RewardType => RewardType.Relic;

    // 结算界面排序：放在遗物(3)之后。
    public override int RewardsSetIndex => 4;

    public override LocString Description => _thought.Title;

    public override bool IsPopulated => _thought != null;

    // 悬停奖励按钮时显示思绪详情（标题 + 描述 + 图标）。
    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        _thought == null
            ? Array.Empty<IHoverTip>()
            : new IHoverTip[] { _thought.HoverTip };

    public override void Populate()
    {
        _thought ??= ThoughtFactory.CreateRandomThought(Player, _rngOverride ?? Player.PlayerRng.Rewards, _roomType);
    }

    public override Control? CreateIcon()
    {
        if (TestMode.IsOn || _thought == null) return null;
        TextureRect icon = new TextureRect
        {
            Texture = _thought.Icon,
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
        };
        icon.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        return icon;
    }

    protected override async Task<bool> OnSelect()
    {
        await ThoughtCmd.Obtain(_thought, Player);
        return true;
    }

    public override void MarkContentAsSeen()
    {
    }
}
