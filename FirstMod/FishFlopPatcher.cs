using HarmonyLib;
using UnityEngine;

namespace FirstMod
{
    [HarmonyPatch(typeof(FlopStates), "ChooseDirection")]
    public class FishFlopPatcher
    {
        private const float interval = 10f;        // Switch interval (game seconds)
        private static float currentDir = -1f;     // Start left
        private static float nextChangeTime = 0f;

        public static void Postfix(FlopStates.Instance smi)
        {
            // Game-time seconds since start (paused time excluded)
            float time = Time.time;

            // Make all flopping fish go in the same direction, alternating every Interval seconds
            // (instead of flopping towards water or randomly flopping)
            if (time >= nextChangeTime)
            {
                currentDir *= -1f;

                nextChangeTime = time + interval;

                Debug.Log($"FirstMod v{HelloWorld.version}: FishFlopPatcher.Postfix(): Direction {(currentDir < 0 ? "left" : "right")} at t={time:F1}");
            }

            smi.currentDir = currentDir;
        }
    }
}
