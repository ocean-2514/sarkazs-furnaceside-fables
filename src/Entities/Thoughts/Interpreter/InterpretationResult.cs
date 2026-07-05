using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model;

namespace Skz.Ocean.Entities.Thoughts.Interpreter;

public class InterpretationResult
{
    public InterpretationResultType ResultType { get; }

    public ThoughtModel? ThoughtResult { get; }

    public RelicModel? RelicResult { get; }

    public int ShieldResult { get; }

    public InterpretationResult(ThoughtModel thoughtResult)
    {
        ResultType = InterpretationResultType.THOUGHT;
        ThoughtResult = thoughtResult;
        RelicResult = null;
        ShieldResult = 0;
    }

    public InterpretationResult(RelicModel relicResult)
    {
        ResultType = InterpretationResultType.RELIC;
        ThoughtResult = null;
        RelicResult = relicResult;
        ShieldResult = 0;
    }

    public InterpretationResult(int shieldResult)
    {
        ResultType = InterpretationResultType.SHIELD;
        ThoughtResult = null;
        RelicResult = null;
        ShieldResult = shieldResult;
    }

    // 无结果（错误/不可解读）。用它代替返回 null，调用方少一处 NPE。
    private InterpretationResult()
    {
        ResultType = InterpretationResultType.NONE;
    }

    public static readonly InterpretationResult None = new InterpretationResult();
}