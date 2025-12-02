using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;
using Verse.AI;

namespace shamblerbeatentodeath
{
    [HarmonyPatch(typeof(Pawn))]
    [HarmonyPatch("ThreatDisabled")]
    [HarmonyPatch(new[] { typeof(IAttackTargetSearcher) })]
    public static class Patch_Pawn_ThreatDisabled
    {
        static bool Prefix(IAttackTargetSearcher disabledFor, ref bool __result)
        {
            // attacker pawn checking if THIS pawn is threat-disabled
            Pawn attacker = (Pawn)disabledFor.Thing; // always a pawn in this context

            // Correct Shambler detection using the HediffDef from Anomaly
            HediffDef shamblerHediff = DefDatabase<HediffDef>.GetNamed("Shambler");
            bool isShambler = attacker.health.hediffSet.HasHediff(shamblerHediff);

            if (isShambler)
            {
                __result = false;   // target is NOT threat-disabled for Shamblers
                return false;       // skip the original ThreatDisabled method
            }

            return true; // non-shambler → run original ThreatDisabled
        }
    }
}
