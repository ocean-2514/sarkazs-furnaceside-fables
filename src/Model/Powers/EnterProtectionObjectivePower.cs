using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;

namespace Skz.Ocean.Model.Powers;

// visual timer
public sealed class EnterProtectionObjectivePower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;
}