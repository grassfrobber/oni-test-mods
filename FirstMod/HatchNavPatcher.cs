using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

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

            // Make adult hatches only able to move according to baby hatch rules (cannot jump up or down or across gaps)
            // Replaces this line from BaseHatchConfig.BaseHatch():
            //     string navGridName = "WalkerNavGrid1x1";
            // With:
            //     string navGridName = "WalkerBabyNavGrid";
            // Which is followed by the baby hatch rules, from where I got the replacement string:
            //     if (is_baby)
            //     {
            //         navGridName = "WalkerBabyNavGrid";
            //     }
            FindReplaceLdstrOperand(codes, "WalkerNavGrid1x1", "WalkerBabyNavGrid");

            Debug.Log($"FirstMod v{HelloWorld.version}: HatchNavPatcher.Transpiler(): Code after changes");

            LogAllInstructions(codes);

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

        public static void FindReplaceLdstrOperand(List<CodeInstruction> codes, string find, string replace)
        {
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand is string s)
                {
                    if (s == find)
                    {
                        Debug.Log($"FirstMod v{HelloWorld.version}: HatchNavPatcher.FindReplaceLdstrOperand(): Found the instruction: #{i}: {codes[i]}\nReplacing operand with: {replace}");

                        codes[i].operand = replace;
                    }
                }
            }
        }
    }
}
