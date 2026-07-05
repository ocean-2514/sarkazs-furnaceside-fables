using System;
using Skz.Ocean.Model;
using Skz.Ocean.Model.Thoughts.Inspirers;
using Skz.Ocean.Model.Thoughts.Lingers;

namespace Skz.Ocean.Entities.Thoughts.Interpreter;

public static class GuaranteedRecipes
{
    public static readonly InterpretationRecipe[] Recipes = new InterpretationRecipe[]
    {
        new InterpretationRecipe(typeof(Reclaim), typeof(Invasion), typeof(RiseAndFall)),
        new InterpretationRecipe(typeof(Reclaim), typeof(EldersCrown), typeof(RiseAndFall)),
        new InterpretationRecipe(typeof(Invasion), typeof(EldersCrown), typeof(RiseAndFall))
    };

    public static bool TryGetThoughtResult(ThoughtModel thought1, ThoughtModel thought2, out Type resultType)
    {
        Type type1 = thought1.GetType();
        Type type2 = thought2.GetType();

        if (type1 == typeof(EldersCrown) && thought2.CanBeUpgraded)
        {
            resultType = thought2.UpgradedType;
            return true;
        } 
        else if (type2 == typeof(EldersCrown) && thought1.CanBeUpgraded)
        {
            resultType = thought1.UpgradedType;
            return true;
        }
        else if (type1 == type2 && thought1.CanBeUpgraded)
        {
            resultType = thought1.UpgradedType;
            return true;
        }

        foreach (var recipe in Recipes)
        {
            if (recipe.Matches(type1, type2))
            {
                resultType = recipe.OutputType;
                return true;
            }
        }

        resultType = null;
        return false;
    }
}