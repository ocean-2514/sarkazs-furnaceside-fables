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

// 58 恶尘源石虫：加入战斗后获得10层CausticDeathBlast；
// 死亡时给所有孽罪奇美拉5层OriginiumStormPower；
// 攻击1(10) -> 攻击2(6*2) 循环
public sealed class CausticOriginiumSlug : SarkazMonsterModel
{
    public override int MaxInitialHp => 34;
    public override int MinInitialHp => 34;

    private const int _damage1 = 10;
    private const int _damage2 = 6;
    private const int _repeat2 = 2;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<CausticDeathBlast>(new ThrowingPlayerChoiceContext(), Creature, 10, Creature, null);
    }

    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Creature) return;
        foreach (Creature enemy in creature.CombatState.Enemies)
        {
            if (enemy == Creature || !enemy.IsAlive) continue;
            if (enemy.Monster is SacrilegiousChimera monster)
            {
                monster.Ignited = true;
                await CreatureCmd.TriggerAnim(enemy, "Skill", 0f);
                await PowerCmd.Apply<OriginiumStormPower>(new ThrowingPlayerChoiceContext(), enemy, 5, Creature, null);
            }
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = attack1;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, attack2 }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).WithHitCount(_repeat2).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
