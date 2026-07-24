using System.Collections.Generic;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using Skz.Ocean.Model.Encounters.Base;
using Skz.Ocean.Model.Monsters;

namespace Skz.Ocean.Model.Encounters.Mocks;

public sealed class MockSpinesOfEpoch : SarkazEncounterModel
{
    public override RoomType RoomType => RoomType.Monster;

    // 必须为 true，否则 NCombatRoom 不会加载 scenes/encounters/mock_spines_of_epoch.tscn，
    // EncounterSlots 为 null，带槽位的怪物入场时会抛异常。
    public override bool HasScene => true;

    // 按屏幕从左到右排列：CombatState 用它给敌人排序（IndexOf 取顺序）。
    public override IReadOnlyList<string> Slots => ["first", "second", "third", "forth", "fifth", "sixth"];

    public override IEnumerable<MonsterModel> AllPossibleMonsters => [
        ModelDb.Monster<SarkazMazeGuide>()
    ];

    protected override IReadOnlyList<IReadOnlyList<WaveEntry>> GenerateWaves()
    {
        return [
            [
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "first"), 
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "second"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "third"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "forth"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "fifth"),
                new WaveEntry(() => ModelDb.Monster<SarkazMazeGuide>(), "sixth")
            ]
        ];
    }
}