using Skz.Ocean.Entities.Ascension;
using Skz.Ocean.Helper;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using Skz.Ocean.Model;

namespace Skz.Ocean.Entities.Thoughts.Interpreter;

public static class Interpreter
{
    public static float FailOdds => SarkazAscensionHelper.GetValueIfAscension(SarkazAscensionLevel.BadLuck, 0.1f, 0.05f);

    public static InterpretationGrade GetGrade(ThoughtModel thought1, ThoughtModel thought2)
    {
        int totalRarity = thought1.Rarity + thought2.Rarity;
        return totalRarity switch
        {
            <= 3 => InterpretationGrade.COMMON,
            <= 5 => InterpretationGrade.UNCOMMON,
            _ => InterpretationGrade.RARE
        };
    }

    public static InterpretationResult Interpret(ThoughtModel thought1, ThoughtModel thought2)
    {
        if (thought1.Owner != thought2.Owner)
        {
            Log.Error("Cannot interpret thoughts from different owners.");
            return InterpretationResult.None;
        }
        return Interpret(thought1, thought2, thought1.Owner.PlayerRng.Rewards);
    }

    
    public static InterpretationResult Interpret(ThoughtModel thought1, ThoughtModel thought2, Rng rng)
    {
        if (thought1.Owner != thought2.Owner)
        {
            Log.Error("Cannot interpret thoughts from different owners.");
            return InterpretationResult.None;
        }

        //check guaranteed recipes first
        if (GuaranteedRecipes.TryGetThoughtResult(thought1, thought2, rng, out var resultType))
        {
            // ModelDbGet 返回 canonical —— ToMutable 后调用方可直接发放。
            ThoughtModel thought = ((ThoughtModel)SarkazHelper.ModelDbGet(resultType)).ToMutable();
            return new InterpretationResult(thought);
        }

        InterpretationGrade grade = GetGrade(thought1, thought2);
        ThoughtType type1 = thought1.ThoughtType;
        ThoughtType type2 = thought2.ThoughtType;

        float relicChance = 0.0f;
        float shieldChance = 0.0f;

        if ((type1 == ThoughtType.Inspirer && type2 == ThoughtType.Inspirer) ||
            (type1 == ThoughtType.Plan && type2 == ThoughtType.Plan))
        {
            relicChance = 0.0f;
            shieldChance = 1.0f;
        }
        else if (type1 == ThoughtType.Linger && type2 == ThoughtType.Linger)
        {
            relicChance = 1.0f;
            shieldChance = 0.0f;
        }
        else if (type1 == ThoughtType.Linger || type2 == ThoughtType.Linger)
        {
            switch (grade)
            {
                case InterpretationGrade.COMMON:
                case InterpretationGrade.UNCOMMON:
                    relicChance = 0.4f;
                    shieldChance = 0.6f;
                    break;
                case InterpretationGrade.RARE:
                    relicChance = 0.5f;
                    shieldChance = 0.5f;
                    break;
            }
        }
        else
        {
            relicChance = 0.5f;
            shieldChance = 0.5f;
        }

        Player player = thought1.Owner;

        return GenerateResult(grade, relicChance, shieldChance, player, rng);
    }

    private static InterpretationResult GenerateResult(InterpretationGrade grade, float relicChance, float shieldChance, Player player, Rng rng)
    {
        if (player == null)
        {
            Log.Error("Player is null when generating interpretation result.");
            return InterpretationResult.None;
        }

        float roll = rng.NextFloat();

        if (roll < relicChance)
        {
            RelicRarity relicRarity = grade switch
            {
                InterpretationGrade.COMMON => RelicRarity.Common,
                InterpretationGrade.UNCOMMON => RelicRarity.Uncommon,
                InterpretationGrade.RARE => RelicRarity.Rare,
                _ => RelicRarity.Common
            };
            // PullNextRelicFromFront 返回 canonical —— ToMutable 后调用方可直接发放。
            RelicModel relic = RelicFactory.PullNextRelicFromFront(player, relicRarity).ToMutable();
            return new InterpretationResult(relic);
        }
        else if (roll <= relicChance + shieldChance)
        {
            int shieldAmount = grade switch
            {
                InterpretationGrade.COMMON => 5,
                InterpretationGrade.UNCOMMON => 10,
                InterpretationGrade.RARE => 15,
                _ => 0
            };
            return new InterpretationResult(shieldAmount);
        }
        else
        {
            Log.Warn("Interpretation result roll did not fall into any category. This should not happen.");
            return InterpretationResult.None;
        }
    }
}
