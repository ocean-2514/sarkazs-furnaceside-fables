using System;

namespace Skz.Ocean.Entities.Thoughts.Interpreter;

public class InterpretationRecipe
{
    public Type InputType1 { get; set; }
    public Type InputType2 { get; set; }

    public Type OutputType { get; set; }

    public InterpretationRecipe(Type inputType1, Type inputType2, Type outputType)
    {
        InputType1 = inputType1;
        InputType2 = inputType2;
        OutputType = outputType;
    }

    public bool Matches(Type inputType1, Type inputType2)
    {
        return (InputType1 == inputType1 && InputType2 == inputType2) ||
               (InputType1 == inputType2 && InputType2 == inputType1);
    }
}