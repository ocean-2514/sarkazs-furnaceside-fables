using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;

namespace Skz.Ocean.Model.Powers;

public sealed class AssassinPower : PowerModel, ISarkazModel
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    private int _powerStack = 2;

    private bool _triggered = false;
    private bool _moved = false;
    private int _moveDelta = 200;

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (target != Owner || _triggered) return;
        if (Owner.CurrentHp <= Owner.MaxHp / 2)
        {
            _triggered = true;
            await PowerCmd.Apply<IntangiblePower>(new ThrowingPlayerChoiceContext(), Owner, _powerStack, null, null);
        }
    }

    public  decimal ModifyMonsterMoveDelta(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster, decimal delta)
    {
        if (monster != Owner.Monster || _moved || !_triggered) return delta;
        _moved = true;
        return delta + _moveDelta;
    }
}