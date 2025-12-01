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
    [StaticConstructorOnStartup]
    public static class ShamblerAggroPatch
    {
        static ShamblerAggroPatch()
        {
            var harmony = new Harmony("mmss.shamblerbeatentodeath");

            // Patch ThinkNode_SeekMeleeTarget.GetPriority
            var thinkNodeType = Type.GetType("RimWorld.ThinkNode_SeekMeleeTarget, Assembly-CSharp");
            var method = thinkNodeType.GetMethod("GetPriority");
            harmony.Patch(method, postfix: new HarmonyMethod(typeof(ShamblerAggroPatch).GetMethod(nameof(ThinkNodePostfix))));

            // Patch JobDriver_MeleeAttack.TryMakePreToilReservations
            var jobDriverType = Type.GetType("RimWorld.JobDriver_MeleeAttack, Assembly-CSharp");
            var jobMethod = jobDriverType.GetMethod("TryMakePreToilReservations");
            harmony.Patch(jobMethod, prefix: new HarmonyMethod(typeof(ShamblerAggroPatch).GetMethod(nameof(JobDriverPrefix))));

            // Patch Verb_MeleeAttack.CanHitTargetFrom
            var verbType = typeof(Verb_MeleeAttack);
            var canHitMethod = verbType.GetMethod("CanHitTargetFrom");
            harmony.Patch(canHitMethod, postfix: new HarmonyMethod(typeof(ShamblerAggroPatch).GetMethod(nameof(VerbPostfix))));
        }

        // Postfix for ThinkNode_SeekMeleeTarget.GetPriority
        public static void ThinkNodePostfix(ref float __result, Pawn pawn)
        {
            if (pawn?.def.defName != "Shambler") return;
            __result += 50f;
        }

        // Prefix for JobDriver_MeleeAttack.TryMakePreToilReservations
        public static bool JobDriverPrefix(object __instance, ref bool __result)
        {
            // Use reflection to get pawn and targetA
            var pawnProp = __instance.GetType().GetProperty("pawn");
            var jobProp = __instance.GetType().GetProperty("job");
            var pawn = (Pawn)pawnProp.GetValue(__instance);
            var job = jobProp.GetValue(__instance);
            var targetAProp = job.GetType().GetProperty("targetA");
            var targetPawn = (LocalTargetInfo)targetAProp.GetValue(job);
            if (pawn?.def.defName == "Shambler" && targetPawn.Pawn != null && targetPawn.Pawn.Downed)
            {
                __result = true;
                return false; // skip original
            }
            return true;
        }

        // Postfix for Verb_MeleeAttack.CanHitTargetFrom
        public static void VerbPostfix(ref bool __result, LocalTargetInfo targ, Pawn attacker)
        {
            if (attacker?.def.defName == "Shambler" && targ.Pawn != null && targ.Pawn.Downed)
            {
                __result = true;
            }
        }
    }
}
