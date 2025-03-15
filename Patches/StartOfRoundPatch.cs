using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Rendering;
using UnityEngine;

namespace MaskedModelReplacement.Patches
{
    internal class StartOfRoundPatch
    {
        [HarmonyPatch(typeof(StartOfRound), "Start")]
        [HarmonyPostfix]
        [HarmonyPriority(Priority.Low)]
        public static void StartOfRoundSuitPatch(StartOfRound __instance)
        {
            if (!MaskedModelReplacementBase.LogAvailableSuits) return;

            StringBuilder sb = new StringBuilder("Available suits:\n");

            int counter = 1;
            var suitList = StartOfRound.Instance.unlockablesList.unlockables;

            foreach (UnlockableItem suit in suitList)
            {
                if (suit.unlockableType != 0) continue;
                sb.Append($"{counter}. {suit.unlockableName.ToLower().Replace(" ", "")}\n");
                counter++;
            }

            MaskedModelReplacementBase.Instance.Logger.LogInfo(sb);
        }
    }
}
