using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace Skz.Ocean.Helper;

public static class ThoughtFx
{
    // 给当前战斗所有我方单位施加 amount 层 T 能力。
    public static async Task ApplyToAllAllies<T>(CombatState combatState, Creature applier, int amount)
        where T : PowerModel
    {
        foreach (Creature ally in combatState.Allies.ToList())
        {
            await PowerCmd.Apply<T>(ally, amount, applier, null);
        }
    }

    // 给所有我方单位增加 pct(0~1) 比例的“本场”最大生命值，返回各自实际增加量（战斗结束用于撤销）。
    public static async Task<Dictionary<Creature, int>> GainTempMaxHpPercent(CombatState combatState, double pct)
    {
        var granted = new Dictionary<Creature, int>();
        foreach (Creature ally in combatState.Allies.ToList())
        {
            int amt = (int)(ally.MaxHp * pct);
            if (amt <= 0) continue;
            await CreatureCmd.GainMaxHp(ally, amt);
            granted[ally] = amt;
        }
        return granted;
    }

    // 撤销之前增加的“本场”最大生命值。
    public static async Task LoseTempMaxHp(Dictionary<Creature, int> granted)
    {
        if (granted == null) return;
        foreach (KeyValuePair<Creature, int> kv in granted)
        {
            if (kv.Key.CurrentHp <= 0) continue; // 已阵亡的不处理
            await CreatureCmd.LoseMaxHp(new ThrowingPlayerChoiceContext(), kv.Key, kv.Value, isFromCard: false);
        }
    }
}