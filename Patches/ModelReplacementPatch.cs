using GameNetcodeStuff;
using HarmonyLib;
using ModelReplacement;
using ModelReplacement.Monobehaviors.Enemies;
using ModelReplacement.Scripts.Enemies;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace MaskedModelReplacement.Patches
{
    // Original code ModelReplacementAPI by BunyaPineTree modified to support custom models for masked enemies.
    internal class ModelReplacementPatch
    {
        public static int NumSpawnedThisLevel = 0;

        [HarmonyPatch(typeof(ModelReplacement.Monobehaviors.Enemies.MaskedReplacementBase), "Awake")]
        [HarmonyPrefix]
        private static bool Awake(ModelReplacement.Monobehaviors.Enemies.MaskedReplacementBase __instance, ref MaskedPlayerEnemy ___enemyAI)
        {
            ___enemyAI = __instance.GetComponent<MaskedPlayerEnemy>();
            
            if (ModelReplacementAPI.MRAPI_NetworkingPresent) return false;

            var mimicking = ___enemyAI.mimickingPlayer;

            bool setReplacement = false;
            BodyReplacementBase modelReplacement = null;
            BodyReplacementBase temporaryModel = null;
            if (mimicking != null)
            {
                setReplacement = ModelReplacementAPI.GetPlayerModelReplacement(mimicking, out var modelReplacement2);
                modelReplacement = modelReplacement2;
            }
            else
            {
                var rand = new System.Random(StartOfRound.Instance.randomMapSeed + NumSpawnedThisLevel);
                var suitList = StartOfRound.Instance.unlockablesList.unlockables;
                UnlockableSuit[] allSuits2 = null;
                var allSuits = Resources.FindObjectsOfTypeAll<UnlockableSuit>().Where(suit => suit.IsSpawned);
                int moon_id = MaskedPlayerEnemyPatch.Get_moon_id();

                if (moon_id > -1)
                {
                    allSuits2 = allSuits
                        .Where(suit => MaskedModelReplacementBase.preferredSuits[moon_id].Contains(suitList[suit.suitID].unlockableName.ToLower().Replace(" ", "")))
                        .Where(suit => MaskedModelReplacementBase.preferredSuits[moon_id][0] != suitList[suit.suitID].unlockableName.ToLower().Replace(" ", ""))
                        .ToArray();

                    if (allSuits2.Length == 0)
                    {
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



                Dictionary<string, Type> regModelRepl = Traverse.Create(typeof(ModelReplacementAPI)).Field("RegisteredModelReplacements").GetValue() as Dictionary<string, Type>;
                
                if (MaskedModelReplacementBase.ModelReplacementsOnly)
                {
                    // filter further if ModelReplacementsOnly is set to true.
                    allSuits2 = allSuits2.Where(suit => regModelRepl.ContainsKey(suitList[suit.suitID].unlockableName.ToLower().Replace(" ", ""))).ToArray();
                }

                if (allSuits2.Length > 0)
                {
                    if (MaskedModelReplacementBase.ShufflePerMoon)
                    {
                        List<UnlockableSuit> allSuits3_pre = new List<UnlockableSuit>();

                        foreach (UnlockableSuit oneSuit in allSuits2)
                        {
                            if (!MaskedModelReplacementBase.ShuffleList.Contains(oneSuit.suitID))
                            {
                                allSuits3_pre.Add(oneSuit);
                            }
                        }

                        if (allSuits3_pre.Count > 0)
                        {
                            allSuits2 = allSuits3_pre.ToArray();
                        }
                    }

                    int suitID = rand.Next(allSuits2.Length);

                    if (MaskedModelReplacementBase.ShufflePerMoon)
                    {
                        MaskedModelReplacementBase.ShuffleList.Add(allSuits2[suitID].suitID);
                    }



                    string suitName = suitList[allSuits2[suitID].suitID].unlockableName;
                    suitName = suitName.ToLower().Replace(" ", "");

                    if (regModelRepl.ContainsKey(suitName))
                    {
                        setReplacement = true;
                        var hostPly = StartOfRound.Instance.allPlayerScripts[0];

                        temporaryModel = hostPly.gameObject.AddComponent(regModelRepl[suitName]) as BodyReplacementBase;
                        temporaryModel.suitName = suitName;
                        modelReplacement = temporaryModel;
                    }
                    else
                    {
                        var target_suit = allSuits2[suitID];
                        ___enemyAI.SetSuit(target_suit.suitID);
                    }
                }
                NumSpawnedThisLevel++;
            }

            if (!setReplacement) { return false; }
            __instance.IsActive = true;

            // Load Models 
            __instance.replacementModel = GameObject.Instantiate(modelReplacement.replacementModel);
            __instance.replacementModel.name += $"(Masked)";
            __instance.replacementModel.transform.localPosition = new Vector3(0, 0, 0);
            __instance.replacementModel.SetActive(true);

            foreach (Renderer renderer in __instance.replacementModel.GetComponentsInChildren<Renderer>())
            {
                renderer.shadowCastingMode = ShadowCastingMode.On;
                renderer.gameObject.layer = modelReplacement.viewState.VisibleLayer;
                renderer.enabled = __instance.enabled;
            }



            // Assign avatars
            MaskedAvatarUpdater mau = new MaskedAvatarUpdater();
            mau.AssignModelReplacement(___enemyAI.gameObject, __instance.replacementModel);
            Traverse.Create(__instance).Field("<avatar>k__BackingField").SetValue(mau);

            // Disable Masked renderers
            ___enemyAI.rendererLOD0.enabled = false;
            ___enemyAI.rendererLOD1.enabled = false;
            ___enemyAI.rendererLOD2.enabled = false;
            ___enemyAI.rendererLOD0.shadowCastingMode = ShadowCastingMode.Off;
            ___enemyAI.rendererLOD1.shadowCastingMode = ShadowCastingMode.Off;
            ___enemyAI.rendererLOD2.shadowCastingMode = ShadowCastingMode.Off;

            // Remove Nametag and Facemask
            MeshRenderer[] gameObjects = ___enemyAI.gameObject.GetComponentsInChildren<MeshRenderer>();
            gameObjects.Where(x => x.gameObject.name == "LevelSticker").First().enabled = false;
            gameObjects.Where(x => x.gameObject.name == "BetaBadge").First().enabled = false;
            gameObjects.Where(x => x.gameObject.name == "ComedyMaskLOD1").First().transform.parent.gameObject.SetActive(false);

            if (temporaryModel != null) {
                temporaryModel.IsActive = false;
                GameObject.Destroy(temporaryModel); // Destroy the existing body replacement
            }

            return false; // Skip original
        }
    }
}
