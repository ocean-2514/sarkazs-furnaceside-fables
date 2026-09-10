namespace Skz.Ocean.Entities.Ascension;

public static class SarkazAscensionState
{
    public const int MinLevel = 0;
    public const int MaxLevel = 10;

    public static int SelectedLevel { get; private set; }

    public static int CurrentLevel { get; private set; }

    public static void Select(int level)
    {
        SelectedLevel = Clamp(level);
    }

    public static void BeginNewRun()
    {
        CurrentLevel = SelectedLevel;
    }

    public static void RestoreRun(int level)
    {
        CurrentLevel = Clamp(level);
    }

    public static void ClearRun()
    {
        CurrentLevel = 0;
    }

    private static int Clamp(int level)
    {
        return System.Math.Clamp(level, MinLevel, MaxLevel);
    }
}