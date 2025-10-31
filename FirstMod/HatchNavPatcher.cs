using HarmonyLib;
using System.Collections.Generic;
using System.Linq;

namespace FirstMod
{
    [HarmonyPatch(typeof(BaseHatchConfig), "BaseHatch")]
    public class HatchNavPatcher
    {
        // From static class BaseHatchConfig, lists the IL code of BaseHatch() for debugging
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            Debug.Log($"FirstMod v{HelloWorld.version}: HatchNavPatcher.Transpiler() for BaseHatchConfig.BaseHatch()");

            List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

            LogAllInstructions(codes);

            // Make no changes to code for now. Converting to List and back to make future editing easy
            return codes.AsEnumerable();
        }

        public static void LogAllInstructions(List<CodeInstruction> codes)
        {
            for (var i = 0; i < codes.Count; i++)
            {
                var code = codes[i];

                Debug.Log($"  Instruction {i}/{codes.Count}: {code}");
                Debug.Log($"    Opcode: {code.opcode}");

                if (code.operand != null)
                {
                    Debug.Log($"    Operand: {code.operand}");
                    Debug.Log($"    Operand type: {code.operand.GetType().FullName}");
                }
            }
        }
    }
}
