using HarmonyLib;
using ModelReplacement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace MaskedModelReplacement.Patches
{
    internal class MaskedPlayerEnemyPatch
    {
        public static int NumSpawnedThisLevel = 0;
        public static int MaxHealth { get; private set; }

        [HarmonyPatch(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.Start))]
        [HarmonyPostfix]
        private static void Start(MaskedPlayerEnemy __instance)
        {
            if (!MaskedModelReplacementBase.modelReplacementPresent) {
                var rand = new System.Random(StartOfRound.Instance.randomMapSeed + NumSpawnedThisLevel);
                var suitList = StartOfRound.Instance.unlockablesList.unlockables;

                var allSuits = Resources.FindObjectsOfTypeAll<UnlockableSuit>()
                    .Where(suit => suit.IsSpawned)
                    .Where(suit => !MaskedModelReplacementBase.MaskedIgnoreSuits.Contains(suitList[suit.suitID].unlockableName.ToLower().Replace(" ", "")))
                    .ToArray();


                var target_suit = allSuits[rand.Next(allSuits.ToArray().Length)];
                MaskedModelReplacementBase.Instance.Logger.LogInfo($"target_suit {target_suit}");
                __instance.SetSuit(target_suit.suitID);
            }

            MaxHealth = __instance.enemyHP;
            NumSpawnedThisLevel++;
        }
    }
}
