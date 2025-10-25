using HarmonyLib;

namespace FirstMod
{
    public class HelloWorld
    {
        public static string version = "0.0.1";

        // Runs before and after Db.Initialize() in Assembly-CSharp.dll
        [HarmonyPatch(typeof(Db))]
        [HarmonyPatch("Initialize")]
        public class Db_Initialize_Patch
        {
            public static void Prefix()
            {
                Debug.Log($"FirstMod v{version}: Hello, world. Before Db.Initialize");
            }

            public static void Postfix()
            {
                Debug.Log($"FirstMod v{version}: After Db.Initialize");
            }
        }
    }
}
