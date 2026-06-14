using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Skz.Ocean.DynamicVars;

public class ToilVar : DynamicVar
{
    public const string defaultName = "Toil";

	public ToilVar(int toil)
		: base("Toil", toil)
	{
	}
}