using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Powers;

public abstract class PassionatePower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    protected virtual int AttackSpeedBuff => 50;
    protected virtual int SwitchDamage => 10;
    protected virtual int StackNum => 5;

    public override async Task AfterApplied(Creature applier, CardModel cardSource)
    {
        await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), Owner, AttackSpeedBuff, null, null);
    }

    public decimal ModifyMoveSpeedMultiplicative(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster, decimal baseSpeed)
    {
        if (monster != Owner.Monster) return 1m;
        return 1.5m;
    }

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (target != Owner || dealer == null || dealer.Side == CombatSide.Enemy) return;
        await PowerCmd.Decrement(this);
    }

    public override async Task AfterRemoved(Creature oldOwner)
    {
        await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), oldOwner, -AttackSpeedBuff, null, null);
        // await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), oldOwner, SwitchDamage, ValueProp.Unblockable | ValueProp.Unpowered, null);
    }
}