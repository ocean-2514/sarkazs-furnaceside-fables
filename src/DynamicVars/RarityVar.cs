using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Skz.Ocean.DynamicVars;

public class RarityVar : DynamicVar
{
    public const string defaultName = "Rarity";

	public RarityVar(int rarity)
		: base("Rarity", rarity)
	{
	}
}