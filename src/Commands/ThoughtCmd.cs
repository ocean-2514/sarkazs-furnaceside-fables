using System;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Helper;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Relics;


namespace Skz.Ocean.Commands;

public static class ThoughtCmd
{
    public static async Task<ThoughtModel> Obtain<T>(Player player) where T : ThoughtModel
    {
        return await Obtain(SarkazHelper.ModelDbGet<T>().ToMutable(), player);
    }

    public static async Task<ThoughtModel> Obtain(ThoughtModel thought, Player player)
    {
        thought.AssertMutable();
        // 随身画笔(150)：新获得的构想负荷-1（入列表前调整，负荷统计从一开始就一致）
        if (thought.ThoughtType == ThoughtType.Plan
            && player.Relics.Any(r => r is PersonalPaintbrush))
        {
            thought.Toil = Math.Max(0, thought.Toil - 1);
        }
        player.AddThoughtInternal(thought);
        thought.FloorAddedToDeck = player.RunState.TotalFloor;
        await thought.AfterObtained();
        return thought;
    }

    public static async Task Remove(ThoughtModel thought)
	{
		thought.Owner.RemoveThoughtInternal(thought);
		await thought.AfterRemoved();
	}

    public static async Task<ThoughtModel> Replace(ThoughtModel original, ThoughtModel replace)
	{
		original.AssertMutable();
		replace.AssertMutable();
		Player player = original.Owner;
		await Remove(original);
		return await Obtain(replace, player);
	}
}