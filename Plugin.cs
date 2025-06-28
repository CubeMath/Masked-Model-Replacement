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
        private const string modVersion = "1.3.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);

        public static MaskedModelReplacementBase Instance;

        public new ManualLogSource Logger;

        public static bool LogAvailableSuits;
        public static bool ModelReplacementsOnly;
        public static bool ShufflePerMoon;
        public static List<string> MaskedIgnoreSuits;
        public static List<List<string>> preferredSuits;
        public static List<string> RackHideSuits;
        public static List<int> ShuffleList;

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

            ShufflePerMoon = Config.Bind(
                "General", "Use Shuffle per Moon", true,
                "This setting causes the the random generator to use Shuffle instead of random only.\n" +
                "The Shuffle-list will be cleared during each round begin.\n"
            ).Value;



            string mergedMaskedIgnoreSuits = Config.Bind(
                "General", "Masked Ignore Suits", "",
                "Comma separated list of suits that the masked enemy should be excluded.\n" +
                "Example: \"default, greensuit, hazardsuit, pajamasuit, purplesuit, beesuit\""
            ).Value;
            MaskedIgnoreSuits = mergedMaskedIgnoreSuits.ToLower().Replace(" ", "").Split(',').Select(s => s.Trim()).ToList();
            
            string preferredSuitsPerMoon = Config.Bind(
                "General", "Preferred Suits per Moon", "",
                "Comma separated list of suits that the masked enemy should wear for specific moons.\n" +
                "This setting will overwrite the \"Masked Ignore Suits\" configuration if entries matches for the current moon.\n" +
                "Example: \"assurance: greensuit hazardsuit pajamasuit,offense: purplesuit beesuit bunnysuit\""
            ).Value;
            var preferredSuitsStrs = preferredSuitsPerMoon.ToLower().Split(',').Select(s => s.Trim()).ToList();

            preferredSuits = new List<List<string>>();
            preferredSuitsStrs.ForEach(suit_str => {
                int colon_idx = suit_str.IndexOf(":");
                if (colon_idx == -1) return;

                List<string> single_suit_list = new List<string>{suit_str.Substring(0, colon_idx)};

                suit_str = suit_str.Substring(colon_idx+1);
                single_suit_list.AddRange(suit_str.Split(' ').Select(s => s.Trim()).ToList());

                preferredSuits.Add(single_suit_list);
            });

            ShuffleList = new List<int>();



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
