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

// 40 萨卡兹迷宫织咒者：debuff(2层Necrosis) -> debuff(2层Necrosis) -> 攻击(20) 循环
public sealed class SarkazMazeSpellweaver : SarkazMonsterModel
{
    public override int MaxInitialHp => 60;
    public override int MinInitialHp => 60;

    private const int _damage = 20;

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff1 = new MoveState("DEBUFF1", Debuff, new DebuffIntent());
        MoveState debuff2 = new MoveState("DEBUFF2", Debuff, new DebuffIntent());
        MoveState attack = new MoveState("ATTACK", Attack, new SingleAttackIntent(_damage));
        debuff1.FollowUpState = debuff2;
        debuff2.FollowUpState = attack;
        attack.FollowUpState = debuff1;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff1, debuff2, attack }, debuff1);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        await CreatureCmd.TriggerAnim(Creature, "Skill", 0f);
        await PowerCmd.Apply<Necrosis>(new ThrowingPlayerChoiceContext(), targets, 2, Creature, null);
    }

    private async Task Attack(IReadOnlyList<Creature> targets)
    {
        await DamageCmd.Attack(_damage).FromMonster(this).WithAttackerAnim("Attack", 0.5f)
            .WithHitFx("vfx/vfx_attack_blunt").Execute(null);
    }
}
