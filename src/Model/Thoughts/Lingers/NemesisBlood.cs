using System;
using System.Threading.Tasks;
using Skz.Ocean.Entities.Thoughts;

namespace Skz.Ocean.Model.Thoughts.Lingers;

// 006 仇人腥血：携带此遗愿进入新一层时，负荷 -1。
public sealed class NemesisBlood : ThoughtModel
{
    protected override int InitialRarity => 3;
    protected override int InitialToil => 5;
    public override ThoughtType ThoughtType => ThoughtType.Linger;
    public override ThoughtRarity ThoughtRarity => ThoughtRarity.Common;

    public override Task AfterActEntered()
    {
        Toil = Math.Max(0, Toil - 1);
        return Task.CompletedTask;
    }
}