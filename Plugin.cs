﻿using BepInEx;
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
        private const string modVersion = "1.1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static MaskedModelReplacementBase Instance;

        public new ManualLogSource Logger;

        public static bool LogAvailableSuits;
        public static bool ModelReplacementsOnly;
        public static List<string> MaskedIgnoreSuits;
        public static List<string> RackHideSuits;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }

            Logger = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            Logger.LogInfo("Init Masked Model Replacement");

            
            modelReplacementPresent = Chainloader.PluginInfos.ContainsKey("meow.ModelReplacementAPI");



            // handle configs
            LogAvailableSuits = Config.Bind(
                "General", "Log Available Suits", false,
                "Log the unlockable names of the available suits into the console when you spawn in the ship.\n" +
                "Names will be written in lowercase and spaces will be removed."
            ).Value;
            
            ModelReplacementsOnly = Config.Bind(
                "General", "Model Replacements Only", false,
                "Masked enemies should only pick model replacements and ignore all suits that does not have a model replacement."
            ).Value;

            string mergedMaskedIgnoreSuits = Config.Bind(
                "General", "Masked Ignore Suits", "",
                "Comma separated list of suits that the masked enemy should be excluded.\n" +
                "Example: \"default, greensuit, hazardsuit, pajamasuit, purplesuit, beesuit\""
            ).Value;
            MaskedIgnoreSuits = mergedMaskedIgnoreSuits.ToLower().Replace(" ", "").Split(',').Select(s => s.Trim()).ToList();
            
            
            
            harmony.PatchAll(typeof(MaskedModelReplacementBase));
            harmony.PatchAll(typeof(MaskedPlayerEnemyPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));

            if (modelReplacementPresent)
            {
                harmony.PatchAll(typeof(ModelReplacementPatch));
            }
        }

        //soft dependencies
        public static bool modelReplacementPresent;
    }
}
