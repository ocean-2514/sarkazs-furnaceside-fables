using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Entities.Players;
using Skz.Ocean.Model;

namespace Skz.Ocean.Runs;

public static class IRunStateExtensions
{
    public static IEnumerable<ISarkazModel> IterateSarkazHookListeners(this IRunState runState)
    {
        foreach (Player player in runState.Players)
        {
            foreach (ThoughtModel thought in player.GetAllThoughts())
            {
                yield return thought;
            }
        }
    }
}
