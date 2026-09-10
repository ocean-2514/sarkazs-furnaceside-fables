using Skz.Ocean.Entities.Ascension;

namespace Skz.Ocean.Helper;

public static class SarkazAscensionHelper
{
    public static bool HasAscension(SarkazAscensionLevel level)
    {
        return SarkazAscensionState.CurrentLevel >= (int)level;
    }

    public static int GetValueIfAscension(
        SarkazAscensionLevel level,
        int ascensionValue,
        int fallbackValue)
    {
        return HasAscension(level) ? ascensionValue : fallbackValue;
    }

    public static float GetValueIfAscension(
        SarkazAscensionLevel level,
        float ascensionValue,
        float fallbackValue)
    {
        return HasAscension(level) ? ascensionValue : fallbackValue;
    }

    public static decimal GetValueIfAscension(
        SarkazAscensionLevel level,
        decimal ascensionValue,
        decimal fallbackValue)
    {
        return HasAscension(level) ? ascensionValue : fallbackValue;
    }
}