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

// 52 残党乐团长笛手：加入战斗后获得5层MelancholicFlutePower；
// 攻击1(11) -> 护盾(13) -> buff 循环
// buff: 如果场上有RemnantOrchestraWindPlayer且其有PassionateWindPower，清除并给5层MelancholicWindPower
public sealed class RemnantOrchestraFlautist : SarkazMonsterModel
{
    public override int MaxInitialHp => 55;
    public override int MinInitialHp => 55;

    private const int _damage = 11;
    private const int _block = 13;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MelancholicFlutePower>(new ThrowingPlayerChoiceContext(), Creature, 5, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        MoveState defend = new MoveState("DEFEND", Defend, new DefendIntent());
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        attack.FollowUpState = defend;
        defend.FollowUpState = buff;
        buff.FollowUpState = attack;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack, defend, buff }, attack);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Defend(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.GainBlock(Creature, _block, MegaCrit.Sts2.Core.ValueProps.ValueProp.Move, null);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        foreach (Creature enemy in Creature.CombatState.Enemies)
        {
            if (!enemy.IsAlive || enemy == Creature) continue;
            if (enemy.Monster is RemnantOrchestraWindPlayer && enemy.HasPower<PassionateWindPower>())
            {
                await PowerCmd.Remove(enemy.GetPower<PassionateWindPower>());
            }
        }
    }
}
