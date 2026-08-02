using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 56 腐败骑士：凋零骑士死亡/离场后自身攻击力+80、攻速+100、移速+150%；
// 加入战斗后获得1层BackAttackLeftPower；
// Buff(10活力) -> 攻击1(20+2易伤) -> 攻击2(20) 循环
public sealed class CorruptedKnight : SarkazMonsterModel
{
    public override int MaxInitialHp => 135;
    public override int MinInitialHp => 135;

    private const int _vigor = 10;
    private const int _damage1 = 20;
    private const int _vuln = 2;
    private const int _damage2 = 20;

    private bool _enraged;

    // 供 WitheredKnight 死亡/逃离时调用
    public async Task Enrage()
    {
        if (_enraged) return;
        _enraged = true;
        await PowerCmd.Apply<AttackPower>(new ThrowingPlayerChoiceContext(), Creature, 80, Creature, null);
        await PowerCmd.Apply<AttackSpeed>(new ThrowingPlayerChoiceContext(), Creature, 100, Creature, null);
        BaseSpeedBonus = InitialSpeed * 1.5m;
    }

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<BackAttackLeftPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    // ISarkazModel：凋零骑士 Escape → 我暴怒
    public override async Task BeforeMonsterEscape(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster)
    {
        if (monster == this) return;
        if (monster is WitheredKnight) await Enrage();
    }

    // 我死亡 → buff 凋零骑士
    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Creature) return;
        foreach (Creature enemy in creature.CombatState.Enemies)
        {
            if (enemy == Creature || !enemy.IsAlive) continue;
            if (enemy.Monster is WitheredKnight withered)
                await withered.Enrage();
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState buff = new MoveState("BUFF", Buff, new BuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        buff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = buff;
        return new MonsterMoveStateMachine(new List<MonsterState> { buff, attack1, attack2 }, buff);
    }

    private async Task Buff(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<VigorPower>(new ThrowingPlayerChoiceContext(), Creature, _vigor, Creature, null);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await PowerCmd.Apply<VulnerablePower>(new ThrowingPlayerChoiceContext(), targets, _vuln, Creature, null);
        await DamageCmd.Attack(_damage1).FromMonster(this).WithAttackerAnim("Skill", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
