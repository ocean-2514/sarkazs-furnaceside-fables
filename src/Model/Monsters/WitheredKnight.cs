using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Afflictions;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 57 凋零骑士：腐败骑士死亡/离场后自身攻击力+80、攻速+100、移速+150%；
// 加入战斗后获得1层BackAttackRightPower；
// 攻击1(7*3) -> buff(2层VitalSpark) -> 攻击2(18) -> 护盾(全体20) 循环
public sealed class WitheredKnight : SarkazMonsterModel
{
    public override int MaxInitialHp => 135;
    public override int MinInitialHp => 135;

    private const int _damage1 = 7;
    private const int _repeat1 = 3;
    private const int _vitalSpark = 2;
    private const int _damage2 = 18;
    private const int _block = 20;

    private bool _enraged;

    // 供 CorruptedKnight 死亡/逃离时调用
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
        await PowerCmd.Apply<BackAttackRightPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    // ISarkazModel：腐败骑士 Escape → 我暴怒
    public override async Task BeforeMonsterEscape(IRunState runState, ICombatState combatState,
        SarkazMonsterModel monster)
    {
        if (monster == this) return;
        if (monster is CorruptedKnight) await Enrage();
    }

    // 我死亡 → buff 腐败骑士
    public override async Task BeforeDeath(Creature creature)
    {
        if (creature != Creature) return;
        foreach (Creature enemy in creature.CombatState.Enemies)
        {
            if (enemy == Creature || !enemy.IsAlive) continue;
            if (enemy.Monster is CorruptedKnight corrupted)
                await corrupted.Enrage();
        }
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new MultiAttackIntent(_damage1, _repeat1));
        MoveState buff = new MoveState("BUFF", Debuff, new DebuffIntent());
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new SingleAttackIntent(_damage2));
        MoveState defend = new MoveState("DEFEND", Defend, new DefendIntent());
        attack1.FollowUpState = buff;
        buff.FollowUpState = attack2;
        attack2.FollowUpState = defend;
        defend.FollowUpState = attack2;
        return new MonsterMoveStateMachine(new List<MonsterState> { attack1, buff, attack2, defend }, attack1);
    }

    private async Task Attack1(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage1).WithHitCount(_repeat1).FromMonster(this).OnlyPlayAnimOnce()
            .WithAttackerAnim("Attack", 0.5f).WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        foreach (Creature item in Creature.CombatState.Allies.ToList())
        {
            if (!item.IsPlayer)
            {
                continue;
            }
            IEnumerable<CardModel> enumerable = item.Player.PlayerCombatState.AllCards.Where((CardModel c) => c.Type == CardType.Skill);
            foreach (CardModel item2 in enumerable)
            {
                await CardCmd.Afflict<Tainted>(item2, _vitalSpark);
            }
        }
        foreach (Creature target in targets)
        {
            await PowerCmd.Apply<VitalSparkPower>(new ThrowingPlayerChoiceContext(), target, _vitalSpark, Creature, null);
        }
    }

    private async Task Attack2(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage2).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }

    private async Task Defend(IReadOnlyList<Creature> targets)
    {
        foreach (Creature enemy in Creature.CombatState.Enemies)
        {
            if (enemy.IsAlive)
                await CreatureCmd.GainBlock(enemy, _block, ValueProp.Move, null);
        }
    }
}
