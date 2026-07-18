using System.Collections.Generic;
using System.Security.AccessControl;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using Skz.Ocean.Model.Relics;

namespace Skz.Ocean.Model.Events;

public sealed class FatedEncounter : EventModel
{
    protected override IReadOnlyList<EventOption> GenerateInitialOptions()
    {
        return [
            new EventOption(this, GainDoodleOfHope, InitialOptionKey("GAIN_DOODLE_OF_HOPE"), HoverTipFactory.FromRelic<DoodleOfHope>()),  
            new EventOption(this, GainTalonsOfHatred, InitialOptionKey("GAIN_TALONS_OF_HATRED"), HoverTipFactory.FromRelic<TalonsOfHatred>()),  
            new EventOption(this, GainLampOfWishes, InitialOptionKey("GAIN_LAMP_OF_WISHES"), HoverTipFactory.FromRelic<LampOfWishes>())  
        ];
    }

    private async Task GainDoodleOfHope()
    {
        await RelicCmd.Obtain<DoodleOfHope>(Owner);
        SetEventFinished(L10NLookup("FATED_ENCOUNTER.pages.GAIN_DOODLE_OF_HOPE.description"));
    }

    private async Task GainTalonsOfHatred()
    {
        await RelicCmd.Obtain<TalonsOfHatred>(Owner);
        SetEventFinished(L10NLookup("FATED_ENCOUNTER.pages.GAIN_TALONS_OF_HATRED.description"));
    }

    private async Task GainLampOfWishes()
    {
        await RelicCmd.Obtain<LampOfWishes>(Owner);
        SetEventFinished(L10NLookup("FATED_ENCOUNTER.pages.GAIN_LAMP_OF_WISHES.description"));
    }
}