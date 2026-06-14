using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model;

namespace Skz.Ocean.Hooks;

public static class SarkazHook
{
    public static async Task AfterThoughtUsed(IRunState runState, ThoughtModel thought)
    {
        foreach (ISarkazModel model in runState.IterateSarkazHookListeners()) //todo
        {
            await model.AfterThoughtUsed();
        }
    }
}