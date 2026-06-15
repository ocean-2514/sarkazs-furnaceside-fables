using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model;
using Skz.Ocean.Runs;

namespace Skz.Ocean.Hooks;

public static class SarkazHook
{
    public static async Task AfterThoughtUsed(IRunState runState, ThoughtModel thought)
    {
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners().ToList())
        {
            await model.AfterThoughtUsed();
        }
    }
}