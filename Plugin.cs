using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using MaskedModelReplacement.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaskedModelReplacement
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("me.swipez.melonloader.morecompany", BepInDependency.DependencyFlags.SoftDependency)]

    public class MaskedModelReplacementBase : BaseUnityPlugin
    {
        private const string modGUID = "CubeMath.MaskedModelReplacementMod";
        private const string modName = "Masked Model Replacement";
        private const string modVersion = "1.0.1.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static MaskedModelReplacementBase Instance;

        public new ManualLogSource Logger;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }

            Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            Logger.LogInfo("Init Masked Model Replacement");

            
            modelReplacementPresent = Chainloader.PluginInfos.ContainsKey("meow.ModelReplacementAPI");

            harmony.PatchAll(typeof(MaskedModelReplacementBase));
            harmony.PatchAll(typeof(MaskedPlayerEnemyPatch));

            if (modelReplacementPresent)
            {
                harmony.PatchAll(typeof(ModelReplacementPatch));
            }
        }

        //soft dependencies
        public static bool modelReplacementPresent;
    }
}
