using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace FirstMod
{
    // Hides the "Building Priority" help popup that appears in the lower-right corner of
    // the screen in the priorities overlay, giving more screen space
    [HarmonyPatch(typeof(OverlayScreen), "ToggleOverlay")]
    public class PriorityScreenPatcher
    {
        // Called when the user clicks the Priorities button or presses the hotkey (default 'P')
        public static void Postfix(HashedString newMode, bool allowSound, OverlayScreen __instance)
        {
            Debug.Log($"FirstMod v{HelloWorld.version}: PriorityScreenPatcher.Postfix(): newMode: {newMode}, allowSound: {allowSound}");

            if (newMode == OverlayModes.Priorities.ID)
            {
                // The "Building Priority" popup isn't immediately added to the Unity UI hierarchy.
                // Use a coroutine to wait until the UI is ready to hide
                Game.Instance.StartCoroutine(HideLegendNextFrame());
            }
        }

        public static IEnumerator HideLegendNextFrame()
        {
            // Wait one frame
            yield return null;

            // Hide the "Building Priority" popup
            DeactivatePriorityLegend();
        }

        public static void DeactivatePriorityLegend()
        {
            // The help overlay is currently available in the Unity GameObject path:
            // ".../ToolMenu/PriorityScreen/OverlayLegend"
            // One way to find this is to use GameObject.Find("OverlayLegend"),
            // but this could find an unrelated UI element, under path
            // ".../UserMenuScreen/PriorityScreen/OverlayLegend"
            // So, find the help overlay using its unique path
            var toolMenu = GameObject.Find("ToolMenu");

            if (toolMenu == null)
            {
                Debug.LogWarning($"FirstMod: ToolMenu not found");
                return;
            }

            var priorityScreen = toolMenu.transform.Find("PriorityScreen");

            if (priorityScreen == null)
            {
                Debug.LogWarning($"FirstMod: PriorityScreen not found");
                return;
            }

            var overlayLegend = priorityScreen.transform.Find("OverlayLegend");

            if (overlayLegend == null)
            {
                Debug.LogWarning($"FirstMod: OverlayLegend not found");
                return;
            }

            Debug.Log($"FirstMod v{HelloWorld.version}: Deactivating {UnityUtil.GetPath(overlayLegend)}");

            overlayLegend.gameObject.SetActive(false);
        }
    }
}
