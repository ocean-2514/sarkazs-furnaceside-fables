using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using MegaCrit.Sts2.Core.MonsterMoves.MonsterMoveStateMachine;
using Skz.Ocean.Model.Monsters.Base;
using Skz.Ocean.Model.Powers;

namespace Skz.Ocean.Model.Monsters;

// 53 残党乐团管乐家：加入战斗后获得5层MelancholicWindPower + 1层WindInstrumentPower；
// 侵蚀(附魔3张Slither) -> 攻击1(20) -> 攻击2(12*2) 循环
public sealed class RemnantOrchestraWindPlayer : SarkazMonsterModel
{
    public override int MaxInitialHp => 68;
    public override int MinInitialHp => 68;

    private const int _damage1 = 20;
    private const int _damage2 = 12;
    private const int _repeat2 = 2;

    public override async Task AfterAddedToRoom()
    {
        await base.AfterAddedToRoom();
        await PowerCmd.Apply<MelancholicWindPower>(new ThrowingPlayerChoiceContext(), Creature, 5, Creature, null);
        await PowerCmd.Apply<WindInstrumentPower>(new ThrowingPlayerChoiceContext(), Creature, 1, Creature, null);
    }

    protected override MonsterMoveStateMachine GenerateMoveStateMachine()
    {
        MoveState debuff = new MoveState("DEBUFF", Debuff, new CardDebuffIntent());
        MoveState attack1 = new MoveState("ATTACK1", Attack1, new SingleAttackIntent(_damage1));
        MoveState attack2 = new MoveState("ATTACK2", Attack2, new MultiAttackIntent(_damage2, _repeat2));
        debuff.FollowUpState = attack1;
        attack1.FollowUpState = attack2;
        attack2.FollowUpState = debuff;
        return new MonsterMoveStateMachine(new List<MonsterState> { debuff, attack1, attack2 }, debuff);
    }

    private async Task Debuff(IReadOnlyList<Creature> targets)
    {
        if (targets.Count == 0) return;
        var player = targets[0].Player;
        if (player == null) return;

        // 收集抽牌堆和弃牌堆中未附魔的牌
        var unenchanted = player.PlayerCombatState.DrawPile.Cards
            .Where(c => c.Enchantment == null)
            .Concat(player.PlayerCombatState.DiscardPile.Cards
                .Where(c => c.Enchantment == null))
            .ToList();

        if (unenchanted.Count == 0) return;

        // 随机最多选3张附魔Slither
        int count = System.Math.Min(3, unenchanted.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = RunRng.MonsterAi.NextInt(unenchanted.Count);
            CardCmd.Enchant<Slither>(unenchanted[idx], 1);
            unenchanted.RemoveAt(idx);
        }
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
