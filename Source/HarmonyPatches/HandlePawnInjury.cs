using System.Reflection;
using HarmonyLib;
using RimWorld;
using VanillaMemesExpanded;
using Verse;

namespace BetterPacifists.HarmonyPatches;

[HarmonyPatch(typeof(DamageWorker_AddInjury), nameof(DamageWorker_AddInjury.Apply))]
internal static class HandlePawnInjury
{
    public static void Prepare(MethodInfo method)
    {
        if (method == null)
            return;

        BetterPacifistsMod.Harmony.Unpatch(method, HarmonyPatchType.Postfix, "com.vanillamemesexpanded");
    }

    public static void Postfix(DamageInfo dinfo, Thing thing, DamageWorker.DamageResult __result)
    {
        if (thing is not Pawn victim || dinfo.Instigator is not Pawn instigator)
            return;

        if (__result.totalDamageDealt <= 0 && !__result.wounded)
            return;

        HistoryEventDef eventDef = null;

        if (victim.RaceProps.Humanlike)
        {
            var notAccident = instigator.Faction?.IsPlayer != true || instigator.drafter?.Drafted == true;

            if (notAccident && !victim.HostileTo(instigator) && instigator.Ideo?.HasPrecept(InternalDefOf.VME_Violence_Abhorrent) == true)
                eventDef = InternalDefOf.VME_AttackedInnocent;
        }
        // Yeah, this one doesn't have as much stuff as the other one
        else if (victim.RaceProps.Animal && instigator.Ideo?.HasPrecept(InternalDefOf.VME_Ranching_Disliked) == true)
            eventDef = InternalDefOf.VME_AttackedAnimal;

        if (eventDef == null)
            return;

        var weapon = dinfo.Weapon;

        var settings = BetterPacifistsMod.settings;
        if (!victim.Downed &&
            (settings.useMaxDamageForNonLethal || __result.totalDamageDealt <= settings.maxDamageForNonLethal) &&
            weapon != null &&
            settings.nonLethalWeapons.Contains(weapon))
        {
            var hediffs = __result.hediffs;
            if (hediffs is { Count: > 0 })
            {
                var damaging = false;

                foreach (var hediff in hediffs)
                {
                    if (!victim.health.CheckPredicateAfterAddingHediff(hediff,
                            () => victim.health.ShouldBeDead() || victim.health.hediffSet.PartIsMissing(hediff.Part)))
                        continue;

                    damaging = true;
                    break;
                }

                if (!damaging)
                    return;
            }
        }

        Find.HistoryEventsManager.RecordEvent(new HistoryEvent(eventDef, dinfo.Instigator.Named(HistoryEventArgsNames.Doer)));
    }
}