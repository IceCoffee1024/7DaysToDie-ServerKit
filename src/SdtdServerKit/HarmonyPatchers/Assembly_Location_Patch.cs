using HarmonyLib;
using System.Reflection;

namespace SdtdServerKit.HarmonyPatchers
{
    [HarmonyPatch]
    internal static class Assembly_Location_Patch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            return AccessTools.PropertyGetter(typeof(int).Assembly.GetType(), nameof(Assembly.Location));
        }

        [HarmonyPostfix]
        private static void Postfix(Assembly __instance, ref string __result)
        {
            if (string.IsNullOrEmpty(__result) && ModApi.ModInstance.ContainsAssembly(__instance))
            {
                __result = Path.Combine(ModApi.ModInstance.Path, __instance.GetName().Name + ".dll");
            }
        }
    }
}
