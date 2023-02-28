using System.Collections.Generic;
using System.Linq;
using BetterPacifists.ModExtensions;
using UnityEngine;
using Verse;

namespace BetterPacifists;

public class Dialog_ConfigureNonLethalWeapons : Window
{
    private BetterPacifistsSettings settings;
    private List<ThingDef> weaponDefs = new();
    private List<ThingDef> searchedWeaponDefs = new();
    private string currentSearch = string.Empty;

    private Vector2 scrollPos = Vector2.zero;

    public override Vector2 InitialSize => new(400f, 700f);
    
    public Dialog_ConfigureNonLethalWeapons(BetterPacifistsSettings settings)
    {
        this.settings = settings;
        doCloseX = true;
    }

    public override void PostOpen()
    {
        base.PostOpen();
        RecalculateWeaponDefs();
    }

    private void RecalculateWeaponDefs()
    {
        weaponDefs.Clear();
        weaponDefs.AddRange(DefDatabase<ThingDef>.AllDefsListForReading.Where(def => def.IsWeapon).OrderBy(def => def.LabelCap.ToString()));
        RecalculateWeaponDefsSearch();
    }

    private void RecalculateWeaponDefsSearch()
    {
        searchedWeaponDefs.Clear();
        if (string.IsNullOrWhiteSpace(currentSearch))
        {
            searchedWeaponDefs.AddRange(weaponDefs);
            return;
        }

        var trimmed = currentSearch.ToLowerInvariant().Trim();
        searchedWeaponDefs.AddRange(weaponDefs.Where(def => def.label.ToLowerInvariant().Trim().Contains(trimmed)).OrderBy(def => def.LabelCap.ToString()));
    }

    public override void DoWindowContents(Rect inRect)
    {
        const float height = 40f;
        
        inRect.yMax -= CloseButSize.y - 4f;
        inRect.yMin += 12f;
        
        var searchBarRect = inRect.TopPartPixels(30f);
        inRect = inRect.BottomPartPixels(inRect.height - 30f);

        var newSearch = Widgets.TextField(searchBarRect, currentSearch);
        if (newSearch != currentSearch)
        {
            currentSearch = newSearch;
            RecalculateWeaponDefsSearch();
        }

        var viewRect = new Rect(0f, 0f, inRect.width - 24f, height * (searchedWeaponDefs.Count + 1) + 10f);
        var listing = new Listing_Extended(inRect, () => scrollPos);
        Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
        listing.Begin(viewRect);
        listing.Gap(10f);

        foreach (var def in searchedWeaponDefs)
        {
            var value = settings.nonLethalWeapons.Contains(def);
            var previous = value;
            listing.CheckboxLabeledIcon(def.LabelCap, ref value, def, null, height);

            if (value == previous) 
                continue;

            if (value)
            {
                settings.nonLethalWeapons.Add(def);
                settings.lethalNonLethalWeapons.Remove(def);
            }
            else
            {
                settings.nonLethalWeapons.Remove(def);
                if (def.HasModExtension<NonLethalByDefault>())
                    settings.lethalNonLethalWeapons.Add(def);
            }
        }

        listing.End();
        Widgets.EndScrollView();
    }
}