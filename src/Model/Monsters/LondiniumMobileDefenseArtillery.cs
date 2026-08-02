using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 50 伦蒂尼姆城防自行炮：debuff(1层ComboedPower) -> 攻击1(3*6) -> 攻击2(20) -> 护盾(10) 循环
public sealed class LondiniumMobileDefenseArtillery : SarkazMonsterModel
{
    public override int MaxInitialHp => 100;
    public override int MinInitialHp => 100;

    public override decimal InitialSpeed => 150;


    private const int _damage1 = 3;
    private const int _repeat1 = 6;
    private const int _damage2 = 20;
    private const int _block = 10;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new DebuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new MultiAttackIntent(_damage1, _repeat1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        MoveState defend = new MoveState("DEFEND", Defend, new DefendIntent());
        debuff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = defend;
        defend.FollowUpState = debuff;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff, attack1, attack2, defend }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<ComboedPower>(new ThrowingPlayerChoiceContext(), targets, 1, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).WithHitCount(_repeat1).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Defend(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, _block, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move, null);
    }
}
