using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace BetterPacifists;

[UsedImplicitly]
public class BetterPacifistsMod : Mod
{
    private static Harmony harmony;
    public static Harmony Harmony => harmony ??= new Harmony("Dra.BetterPacifists");

    public static BetterPacifistsSettings settings;

    public BetterPacifistsMod(ModContentPack content) : base(content)
    {
        LongEventHandler.ExecuteWhenFinished(() => settings = GetSettings<BetterPacifistsSettings>());
        Harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect) => settings.DoSettingsWindowContents(inRect);

    public override string SettingsCategory() => "Vanilla Ideology Expanded Addon - Better Pacifists";
}