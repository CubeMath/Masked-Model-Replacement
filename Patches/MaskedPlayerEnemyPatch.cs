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

        public static int Get_moon_id() {
            String str = StartOfRound.Instance.currentLevel.PlanetName.ToLower();

            int space_idx = str.IndexOf(" ");
            String str2 = null;
            if (space_idx > -1) {
                str2 = str.Substring(space_idx+1);
            }

            int found_idx = -1;
            int i = 0;
            MaskedModelReplacementBase.preferredSuits.ForEach(pSuit => {
                if (found_idx != -1) return;
                if (pSuit[0] == str || pSuit[0] == str2)
                {
                    found_idx = i;
                    return;
                }
                i++;
            });

            return found_idx;
        }

        [HarmonyPatch(typeof(MaskedPlayerEnemy), nameof(MaskedPlayerEnemy.Start))]
        [HarmonyPostfix]
        private static void Start(MaskedPlayerEnemy __instance)
        {
            if (!MaskedModelReplacementBase.modelReplacementPresent) {
                var rand = new System.Random(StartOfRound.Instance.randomMapSeed + NumSpawnedThisLevel);
                var suitList = StartOfRound.Instance.unlockablesList.unlockables;
                UnlockableSuit[] allSuits2 = null;
                var allSuits = Resources.FindObjectsOfTypeAll<UnlockableSuit>().Where(suit => suit.IsSpawned);
                int moon_id = Get_moon_id();

                if (moon_id > -1)
                {
                    allSuits2 = allSuits
                        .Where(suit => MaskedModelReplacementBase.preferredSuits[moon_id].Contains(suitList[suit.suitID].unlockableName.ToLower().Replace(" ", "")))
                        .Where(suit => MaskedModelReplacementBase.preferredSuits[moon_id][0] != suitList[suit.suitID].unlockableName.ToLower().Replace(" ", ""))
                        .ToArray();

                    if (allSuits2.Length == 0) {
                        allSuits2 = allSuits
                            .Where(suit => !MaskedModelReplacementBase.MaskedIgnoreSuits.Contains(suitList[suit.suitID].unlockableName.ToLower().Replace(" ", "")))
                            .ToArray();
                    }
                }
                else
                {
                    allSuits2 = allSuits
                        .Where(suit => !MaskedModelReplacementBase.MaskedIgnoreSuits.Contains(suitList[suit.suitID].unlockableName.ToLower().Replace(" ", "")))
                        .ToArray();
                }

                var target_suit = allSuits2[rand.Next(allSuits2.ToArray().Length)];
                MaskedModelReplacementBase.Instance.Logger.LogInfo($"target_suit {suitList[target_suit.suitID].unlockableName.ToLower().Replace(" ", "")}");
                __instance.SetSuit(target_suit.suitID);
            }

            MaxHealth = __instance.enemyHP;
            NumSpawnedThisLevel++;
        }
    }
}
