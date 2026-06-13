using HarmonyLib;
using System;
using System.IO;
using System.Reflection;

namespace SdtdServerKit.HarmonyPatchers
{
    [HarmonyPatch]
    internal static class Assembly_Location_Patch
    {
        [HarmonyTargetMethod]
        private static MethodBase TargetMethod()
        {
            var assemblyType = typeof(int).Assembly.GetType();
            return AccessTools.PropertyGetter(assemblyType, nameof(Assembly.Location));
        }

        [HarmonyPostfix]
        private static void Postfix(Assembly __instance, ref string __result)
        {
            if (!string.IsNullOrEmpty(__result)) return;

            if (!ModApi.ModInstance.ContainsAssembly(__instance)) return;

            if (!string.IsNullOrEmpty(__instance.CodeBase))
            {
                try
                {
                    var uri = new Uri(__instance.CodeBase);
                    if (uri.IsFile)
                    {
                        __result = uri.LocalPath;
                        return;
                    }
                }
                catch { }
            }

            __result = Path.Combine(ModApi.ModInstance.Path, __instance.GetName().Name + ".dll");

            if (!File.Exists(__result))
            {
                CustomLogger.Debug($"构造的程序集路径不存在: {__result}");
            }
        }
    }
}