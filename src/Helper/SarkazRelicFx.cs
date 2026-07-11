using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace Skz.Ocean.Helper;

// 遗物通用效果（对照原版 DustyTome / SmithRestSiteOption 的惯用法）。
public static class SarkazRelicFx
{
    // 给任意目标施加能力（遗物没有 PlayerChoiceContext 的场合用 Throwing 占位，同原版怪物写法）。
    public static Task ApplyPower<T>(RelicModel relic, Creature target, decimal amount) where T : PowerModel
    {
        return PowerCmd.Apply<T>(new ThrowingPlayerChoiceContext(), target, amount, relic.Owner.Creature, null);
    }

    // 给持有者自身施加能力。
    public static Task ApplySelf<T>(RelicModel relic, decimal amount) where T : PowerModel
    {
        return ApplyPower<T>(relic, relic.Owner.Creature, amount);
    }

    // “基础生命值+pct%”：按角色 StartingHp 计算增量，永久提高生命上限。
    public static async Task GainBaseHpPercent(Player player, int percent)
    {
        int amount = player.Character.StartingHp * percent / 100;
        if (amount > 0)
        {
            await CreatureCmd.GainMaxHp(player.Creature, amount);
        }
    }

    // 拾取时选 count 张牌升级（filter 可限定牌类型）。牌不足时能选几张升几张。
    public static async Task UpgradeCardsViaSelection(Player player, int count, Func<CardModel, bool> filter = null)
    {
        CardSelectorPrefs prefs = new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, count)
        {
            RequireManualConfirmation = true,
        };
        IEnumerable<CardModel> selection = filter == null
            ? await CardSelectCmd.FromDeckForUpgrade(player, prefs)
            : await CardSelectCmd.FromDeckGeneric(player, prefs, c => c.IsUpgradable && filter(c));
        foreach (CardModel card in selection)
        {
            CardCmd.Upgrade(card);
        }
    }

    // 随机获得一张升级过的牌加入牌组（filter 可限定稀有度等），带入组预览。
    public static async Task GainRandomUpgradedCard(Player player, Func<CardModel, bool> filter = null)
    {
        List<CardModel> pool = player.Character.CardPool
            .GetUnlockedCards(player.UnlockState, player.RunState.CardMultiplayerConstraint)
            .Where(c => c.IsUpgradable && (filter == null || filter(c)))
            .ToList();
        if (pool.Count == 0)
        {
            return;
        }
        CardModel canonical = player.PlayerRng.Rewards.NextItem(pool);
        CardModel card = player.RunState.CreateCard(canonical, player);
        CardCmd.Upgrade(card);
        CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck), 2f);
    }

    public static async Task LoseMaxHp(Creature creature, int amount)
    {
        if (amount < 0 || amount >= creature.MaxHp)
        {
            throw new InvalidOperationException("LoseMaxHp but amount is negative/amount >= MaxHp(should not happen)");
        }
        float ratio = (float)creature.CurrentHp / (float)creature.MaxHp;
        int newHp = Math.Max((int)((creature.MaxHp - amount) * ratio), 1);
        await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), creature, (decimal)creature.CurrentHp - newHp, (ValueProp.Unblockable | ValueProp.Unpowered), null, null);
        await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), creature, amount, false);
    }
}
