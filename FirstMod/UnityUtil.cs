using UnityEngine;

namespace FirstMod
{
    public class UnityUtil
    {
        public static void DumpHierarchy(Transform t, string indent = "")
        {
            Debug.Log($"{indent}{t.name}");

            foreach (Transform child in t)
                DumpHierarchy(child, indent + "  ");
        }

        public static string GetPath(Transform t)
        {
            return t.parent == null ? t.name : GetPath(t.parent) + "/" + t.name;
        }
    }
}
