using System.Collections.Generic;
using BetterPacifists.ModExtensions;
using UnityEngine;
using Verse;

namespace BetterPacifists;

public class BetterPacifistsSettings : ModSettings
{
    public bool fistsCountAsNonLethal = false;
    public bool useMaxDamageForNonLethal = true;
    public int maxDamageForNonLethal = 5;

    public HashSet<ThingDef> nonLethalWeapons;
    public HashSet<ThingDef> lethalNonLethalWeapons;

    public override void ExposeData()
    {
        Scribe_Values.Look(ref fistsCountAsNonLethal, "fistsCountAsNonLethal", false);
        Scribe_Values.Look(ref useMaxDamageForNonLethal, "useMaxDamageForNonLethal", true);
        Scribe_Values.Look(ref maxDamageForNonLethal, "maxDamageForNonLethal", 5);

        Scribe_Collections.Look(ref nonLethalWeapons, "nonLethalWeapons", LookMode.Def);
        Scribe_Collections.Look(ref lethalNonLethalWeapons, "lethalNonLethalWeapons", LookMode.Def);

        nonLethalWeapons ??= new HashSet<ThingDef>();
        lethalNonLethalWeapons ??= new HashSet<ThingDef>();

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
            CheckDefsForExtensions();
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard
        {
            ColumnWidth = 400f
        };
        listing.Begin(inRect);

        if (listing.ButtonText("BetterPacifists.EditNonLethalWeapons".Translate()))
            Find.WindowStack.Add(new Dialog_ConfigureNonLethalWeapons(this));

        listing.Gap();

        listing.CheckboxLabeled("BetterPacifists.FistsNonLethal".Translate(), ref fistsCountAsNonLethal, "BetterPacifists.FistsNonLethal.Tooltip".Translate());
        listing.CheckboxLabeled("BetterPacifists.UseMaxDamageForNonLethal".Translate(), ref useMaxDamageForNonLethal, "BetterPacifists.UseMaxDamageForNonLethal.Tooltip".Translate());
        listing.Label("BetterPacifists.MaxDamageForNonLethal".Translate(), tooltip: "BetterPacifists.MaxDamageForNonLethal.Tooltip".Translate());
        string buffer = null;
        listing.IntEntry(ref maxDamageForNonLethal, ref buffer);
        if (maxDamageForNonLethal < 0)
            maxDamageForNonLethal = 0;
        
        listing.Gap();

        if (listing.ButtonText("BetterPacifists.ResetToDefault".Translate()))
            ResetToDefaults();

        listing.End();
    }

    private void ResetToDefaults()
    {
        fistsCountAsNonLethal = false;
        useMaxDamageForNonLethal = true;
        maxDamageForNonLethal = 5;

        // Clear and then fill them again
        nonLethalWeapons.Clear();
        lethalNonLethalWeapons.Clear();
        CheckDefsForExtensions();
    }

    private void CheckDefsForExtensions()
    {
        foreach (var def in DefDatabase<ThingDef>.AllDefs)
        {
            if (!def.IsWeapon)
                continue;

            var hasExtension = def.HasModExtension<NonLethalByDefault>();

            if (!hasExtension)
            {
                // If the weapon doesn't have extension, just ensure it's not considered "lethal non-lethal" weapon.
                lethalNonLethalWeapons.Remove(def);
                continue;
            }

            // If the weapon has the extension and isn't on any of the 2 lists, add it to non-lethal ones
            if (!nonLethalWeapons.Contains(def) && !lethalNonLethalWeapons.Contains(def))
                nonLethalWeapons.Add(def);
        }
    }
}