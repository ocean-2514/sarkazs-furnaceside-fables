using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Helper;
using Skz.Ocean.Model;


namespace Skz.Ocean.Commands;

public static class ThoughtCmd
{
    public static async Task<ThoughtModel> Obtain<T>(Player player) where T : ThoughtModel
    {
        return (T)(await Obtain(SarkazHelper.ModelDbGet<T>(), player));
    }

    public static async Task<ThoughtModel> Obtain(ThoughtModel thought, Player player)
    {
        thought.AssertMutable();
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