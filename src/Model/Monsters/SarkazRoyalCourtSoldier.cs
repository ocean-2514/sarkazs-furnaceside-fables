using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 39 萨卡兹王庭军战士：加入战斗后获得1层NecrosisAttackPower；
// 攻击(12) -> debuff(加虚空到弃牌堆) -> 护盾(9) 循环
public sealed class SarkazRoyalCourtSoldier : SarkazMonsterModel
{
    public override int MaxInitialHp => 35;
    public override int MinInitialHp => 35;

    private const int _damage = 12;
    private const int _block = 9;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<NecrosisAttackPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        MoveState debuff = new MoveState("DEBUFF", Debuff, new CardDebuffIntent());
        MoveState defend = new MoveState("DEFEND", Defend, new DefendIntent());
        attack.FollowUpState = debuff;
        debuff.FollowUpState = defend;
        defend.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack, debuff, defend }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await CardPileCmd.AddToCombatAndPreview<Void>(targets, PileType.Discard, 1, null);
    }

    private async Task Defend(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, _block, ValueProp.Move, null);
    }
}
