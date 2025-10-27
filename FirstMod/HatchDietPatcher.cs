using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FirstMod
{
    // Modifies the diet of regular baby and adult hatches to exclude duplicant foods
    // (ration bars, mush bars, uncooked and cooked meals, etc.) while still including
    // mineral "foods" (sand, sandstone, etc.). Sage hatches are unaffected.
    [HarmonyPatch(typeof(HatchConfig))]
    [HarmonyPatch("CreateHatch")]
    public class HatchDietPatcher
    {
        // Runs after HatchConfig.CreateHatch(), which is called only twice. During game setup, before
        // a world is loaded, CreateHatch() is called once for adult regular hatches,
        // and once for baby regular hatches
        public static void Postfix(
            string id, string name, string desc, string anim_file, bool is_baby,
            ref GameObject __result, HatchConfig __instance)
        {
            Debug.Log($"FirstMod v{HelloWorld.version}: HatchDietPatcher.Postfix(): Starting for {id} critters");

            // CreateHatch() returned a Unity GameObject that contains the hatch's diet. We'll keep
            // this GameObject intact, but give it a new diet.
            // Get the SolidConsumerMonitor object attached to the GameObject
            var monitor = __result.GetDef<SolidConsumerMonitor.Def>();

            // The monitor contains a Diet, which contains an array of Diet.Info objects
            var diet = monitor.diet;
            var infos = diet.infos;

            // Debug
            var oldCount = infos.Count();
            var oldTagNames = new List<string>();
            var newTagNames = new List<string>();

            // Prepare to reconstruct the Diet object
            List<Diet.Info> newDiet = new List<Diet.Info>();

            foreach (var info in infos)
            {
                // Each Diet.Info object has a HashSet of or more "tags" (enum values from SimHashes)
                var consumedTags = info.consumedTags;

                // CreateHatch() created one Diet.Info object with several minerals -
                // Sand, Sandstone, etc. It then created many Diet.Info objects, each with an
                // individual duplicant food (FieldRation, MushBar, etc.). Include the mineral-based
                // Diet.Info in the new diet, and exclude the others
                bool isRockDiet = false;

                foreach (var tag in consumedTags)
                {
                    if (tag.ToString() == "Sand")
                    {
                        isRockDiet = true;
                        
                        // TODO: break disabled for now to let debugging below work
                        // break;
                    }

                    // Debug
                    oldTagNames.Add(tag.ToString());
                }

                if (isRockDiet)
                {
                    newDiet.Add(info);
                }
            }

            // Debug
            foreach (var newDietInfo in newDiet)
            {
                foreach (var tag in newDietInfo.consumedTags)
                {
                    newTagNames.Add(tag.ToString());
                }
            }

            // Redo the SetupDiet() call from CreateHatch()

            // Get the value of HatchConfig.CALORIES_PER_KG_OF_ORE
            var calories_accessor = AccessTools.Field(typeof(HatchConfig), "CALORIES_PER_KG_OF_ORE");
            float CALORIES_PER_KG_OF_ORE = (float)calories_accessor.GetValue(null);

            // Get the value of HatchConfig.MIN_POOP_SIZE_IN_KG
            var poop_accessor = AccessTools.Field(typeof(HatchConfig), "MIN_POOP_SIZE_IN_KG");
            float MIN_POOP_SIZE_IN_KG = (float)poop_accessor.GetValue(null);

            GameObject gameObject = BaseHatchConfig.SetupDiet(
                __result,
                newDiet,
                CALORIES_PER_KG_OF_ORE,
                MIN_POOP_SIZE_IN_KG
            );

            // Debug
            var newCount = newDiet.Count;
            Debug.Log($"FirstMod v{HelloWorld.version}: HatchDietPatcher.Postfix(): Ending. Results:");
            Debug.Log($"- Old diet list length: {oldCount}. New: {newCount}");
            Debug.Log($"- Old diet tag length: {oldTagNames.Count}. New: {newTagNames.Count}");
            Debug.Log($"- Old diet: {string.Join(", ", oldTagNames)}");
            Debug.Log($"- New diet: {string.Join(", ", newTagNames)}");
        }
    }
}
