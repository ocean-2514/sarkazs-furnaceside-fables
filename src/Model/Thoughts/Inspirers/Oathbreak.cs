using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Entities.Thoughts;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Thoughts.Inspirers;

// 026 背誓：使用后下次战斗所有我方单位受到伤害后，攻击速度 +50，持续 2 回合(不可叠加)。
public sealed class Oathbreak : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 4;
    public override ThoughtType ThoughtType => ThoughtType.Inspirer;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Uncommon;

    private const int _amount = 50;
    private const int _duration = 2;
    private readonly Dictionary<Creature, int> _timers = new Dictionary<Creature, int>();

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target,
        DamageResult result, ValueProp props, Creature dealer, CardModel cardSource)
    {
        if (Status != ThoughtStatus.Used) return;
        if (target.Side != Owner.Creature.Side) return; // 仅我方
        if (result.UnblockedDamage <= 0) return;
        if (_timers.ContainsKey(target)) return;         // 不可叠加
        _timers[target] = _duration;
        await PowerCmd.Apply<AttackSpeed>(target, _amount, Owner.Creature, null);
    }

    public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
    {
        if (Status != ThoughtStatus.Used || side != Owner.Creature.Side) return;
        foreach (Creature c in _timers.Keys.ToList())
        {
            _timers[c]--;
            if (_timers[c] <= 0)
            {
                _timers.Remove(c);
                await PowerCmd.Apply<AttackSpeed>(c, -_amount, Owner.Creature, null);
            }
        }
    }
}