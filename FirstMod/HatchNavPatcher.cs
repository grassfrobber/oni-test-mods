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

            // Make adult hatches use baby hatch navigation (no jumping or gap traversal).
            // Replaces this from BaseHatchConfig.BaseHatch():
            //     string navGridName = "WalkerNavGrid1x1";
            // With:
            //     string navGridName = "WalkerBabyNavGrid";
            // Which is followed by the baby hatch rules, which gave the replacement string:
            //     if (is_baby)
            //     {
            //         navGridName = "WalkerBabyNavGrid";
            //     }
            FindReplaceLdstrOperandOnce(codes, "WalkerNavGrid1x1", "WalkerBabyNavGrid");

            // BUGGED: Make baby hatches use adult navigation rules.
            // Causes baby hatches to freeze when trying to jump (due to missing animations).
            // Included for educational purposes to demonstrate multiple IL edits.
            // Replaces this:
            //     if (is_baby)
            //     {
            //         navGridName = "WalkerBabyNavGrid";
            //     }
            // With:
            //     if (is_baby)
            //     {
            //         navGridName = "WalkerNavGrid1x1";
            //     }
            FindReplaceLdstrOperandOnce(codes, "WalkerBabyNavGrid", "WalkerNavGrid1x1", 2);

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

        public static void FindReplaceLdstrOperandOnce(List<CodeInstruction> codes, string find, string replace, int occurrence = 1)
        {
            int currentOccurrence = 1;

            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ldstr && codes[i].operand is string s)
                {
                    if (s == find)
                    {
                        if (currentOccurrence == occurrence)
                        {
                            Debug.Log($"FirstMod v{HelloWorld.version}: HatchNavPatcher.FindReplaceLdstrOperandOnce(): Found the instruction: #{i}: {codes[i]}\nReplacing operand with: {replace}");

                            codes[i].operand = replace;

                            return;
                        }
                        else
                        {
                            Debug.Log($"FirstMod v{HelloWorld.version}: HatchNavPatcher.FindReplaceLdstrOperandOnce(): Skipping occurrence {currentOccurrence} of #{i}: {codes[i]}");
                        }

                        currentOccurrence++;
                    }
                }
            }
        }
    }
}
