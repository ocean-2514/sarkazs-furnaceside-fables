using System;
using System.Collections.Generic;
using System.Text.Json;
using HarmonyLib;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Logging;

namespace Skz.Ocean.Patches;

// 基础游戏的 loc 加载只会把 mod 的 loc 表“合并”到已存在的基础表名上
// （它遍历基础游戏的 loc 文件，再对每个文件名调 GetModdedLocTables）。
// “thoughts” 是基础游戏没有的新表 → 永远不会被加载 → LocManager.GetTable("thoughts") 抛异常。
// 这里在 SetLanguage 之后，把 mod 自己的 thoughts.json（带 eng 兜底）注入 LocManager._tables。
[HarmonyPatch(typeof(LocManager), nameof(LocManager.SetLanguage))]
public static class ThoughtsLocPatch
{
    private const string ModId = "SarkazsFurnacesideFables";
    private const string ThoughtTableName = "thoughts";
    private const string ShieldTableName = "shield";

    public static void Postfix(LocManager __instance, string language)
    {
        Inject(__instance, language, ThoughtTableName);
        Inject(__instance, language, ShieldTableName);
    }

    // 供 ModInitializer 调用：覆盖“mod 初始化晚于 LocManager 构造”的情况
    // （那时初始 SetLanguage 已经跑过、没被 patch 到）。
    public static void InjectIntoCurrent()
    {
        LocManager inst = LocManager.Instance;
        if (inst != null)
        {
            Inject(inst, inst.Language, ThoughtTableName);
            Inject(inst, inst.Language, ShieldTableName);
        }
    }

    private static void Inject(LocManager locManager, string language, string tableName)
    {
        Dictionary<string, LocTable> tables = Traverse.Create(locManager)
            .Field("_tables").GetValue<Dictionary<string, LocTable>>();
        if (tables == null)
        {
            return;
        }

        // 非英文时，准备 eng 兜底，缺键时回退到英文
        LocTable engFallback = null;
        if (language != "eng")
        {
            Dictionary<string, string> engData = LoadModTable("eng", tableName);
            if (engData != null)
            {
                engFallback = new LocTable(tableName, engData);
            }
        }

        Dictionary<string, string> data = LoadModTable(language, tableName) ?? new Dictionary<string, string>();
        tables[tableName] = new LocTable(tableName, data, engFallback);
        Log.Info($"[Sarkaz] Injected '{tableName}' loc table for language '{language}'");
    }

    private static Dictionary<string, string> LoadModTable(string language, string tableName)
    {
        // 和 ModManager.GetModdedLocTables 一致的路径约定
        string path = $"res://{ModId}/localization/{language}/{tableName}.json";
        if (!Godot.FileAccess.FileExists(path))
        {
            return null;
        }
        using Godot.FileAccess file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            return null;
        }
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(file.GetAsText());
        }
        catch (Exception e)
        {
            Log.Error($"[Sarkaz] Failed to parse {path}: {e}");
            return null;
        }
    }
}
