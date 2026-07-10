// using System.Collections.Generic;
// using System.Threading.Tasks;
// using MegaCrit.Sts2.Core.Commands;
// using MegaCrit.Sts2.Core.Entities.Cards;
// using MegaCrit.Sts2.Core.Events;
// using MegaCrit.Sts2.Core.Localization.DynamicVars;
// using MegaCrit.Sts2.Core.Models;
// using MegaCrit.Sts2.Core.ValueProps;
// using TestMod.src.relics;
// using TestMod.src.cards;
// using TestMod.src.potions;
// using MegaCrit.Sts2.Core.GameActions.Multiplayer;
// using MegaCrit.Sts2.Core.HoverTips;
// using MegaCrit.Sts2.Core.Localization;
// using MegaCrit.Sts2.Core.Logging;

// namespace TestMod.src.events;

// public sealed class MyCustomEvent : EventModel {
//     protected override IEnumerable<DynamicVar> CanonicalVars => [
//         new DamageVar(5m, ValueProp.Unblockable | ValueProp.Unpowered)
//     ];

//     protected override List<EventOption> GenerateInitialOptions() {
//         return [
//             new EventOption(this, GainRelic, InitialOptionKey("GAIN_RELIC"), HoverTipFactory.FromRelic<MyCustomRelic>()),
//             new EventOption(this, GainCard, InitialOptionKey("GAIN_CARD"), HoverTipFactory.FromCard<MyCustomCard>()),
//             new EventOption(this, GainPotion, InitialOptionKey("GAIN_POTION"), HoverTipFactory.FromPotion<MyCustomPotion>()),
//             new EventOption(this, TryToGainRelic, InitialOptionKey("TRY_TO_GAIN_RELIC"), HoverTipFactory.FromRelic<MyCustomRelic>())
//                 .ThatDoesDamage(DynamicVars.Damage.BaseValue)
//         ];
//     }

//     private async Task GainRelic() {
//         await RelicCmd.Obtain<MyCustomRelic>(Owner);
//         await Exit();
//     }

//     private async Task GainCard() {
//         CardModel card = Owner.RunState.CreateCard<MyCustomCard>(Owner);
//         CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
//         await Exit();
//     }

//     private async Task GainPotion() {
//         await PotionCmd.TryToProcure<MyCustomPotion>(Owner);
//         await Exit();
//     }

//     private async Task TryToGainRelic() {
//         await TakeAndIncreaseDamage();
//         await RelicCmd.Obtain<MyCustomRelic>(Owner);
//         //debug

//         SetEventState(L10NLookup("MY_CUSTOM_EVENT.pages.TRY_TO_GAIN_RELIC.description"), new List<EventOption>([
//             new EventOption(this, TryToGainCard, "MY_CUSTOM_EVENT.pages.ALL.options.TRY_TO_GAIN_CARD", 
//                 HoverTipFactory.FromCard<MyCustomCard>()),
//             new EventOption(this, Exit, "MY_CUSTOM_EVENT.pages.ALL.options.EXIT")
//         ])); 
//     }

//     private async Task TryToGainCard() {
//         await TakeAndIncreaseDamage();
//         CardModel card = Owner.RunState.CreateCard<MyCustomCard>(Owner);
//         CardCmd.PreviewCardPileAdd(await CardPileCmd.Add(card, PileType.Deck));
//         SetEventState(L10NLookup("MY_CUSTOM_EVENT.pages.TRY_TO_GAIN_CARD.description"), new List<EventOption>([
//             new EventOption(this, TryToGainPotion, "MY_CUSTOM_EVENT.pages.ALL.options.TRY_TO_GAIN_POTION", 
//                 HoverTipFactory.FromPotion<MyCustomPotion>()),
//             new EventOption(this, Exit, "MY_CUSTOM_EVENT.pages.ALL.options.EXIT")
//         ]));
//     }

//     private async Task TryToGainPotion() {
//         await TakeAndIncreaseDamage();
//         await PotionCmd.TryToProcure<MyCustomPotion>(Owner);
//         SetEventFinished(L10NLookup("MY_CUSTOM_EVENT.pages.TRY_TO_GAIN_POTION.description"));
//     }

//     private async Task Exit() {
//         SetEventFinished(L10NLookup("MY_CUSTOM_EVENT.pages.EXIT.description"));
//         await Task.CompletedTask;
//     }

//     private async Task TakeAndIncreaseDamage() {
//         await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), Owner.Creature, DynamicVars.Damage, null, null);
//         DynamicVars.Damage.BaseValue += 5m;
//     }
// }