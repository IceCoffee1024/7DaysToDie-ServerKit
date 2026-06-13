using HarmonyLib;
using System.Reflection;

namespace SdtdServerKit.HarmonyPatchers
{
    [HarmonyPatch(typeof(Assembly))]
    internal static class AssemblyPatcher
    {
        private static readonly Func<Assembly, bool, Type[]> _originalGetTypes =
            AccessTools.MethodDelegate<Func<Assembly, bool, Type[]>>(typeof(Assembly)
                .GetMethod(nameof(Assembly.GetTypes), BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(bool) }, null));

        [HarmonyPrefix]
        [HarmonyPatch(nameof(Assembly.GetTypes), new Type[] { })]
        public static bool Before_GetTypes(Assembly __instance, ref Type[] __result)
        {
            try
            {
                var executingAssembly = typeof(AssemblyPatcher).Assembly;

                // Check if the assembly is the executing assembly
                if (__instance == executingAssembly)
                {
                    return true;
                }

                if (__instance.IsDynamic)
                {
                    goto Return;
                }

                if (File.Exists(__instance.Location) && Path.GetDirectoryName(__instance.Location) == Path.GetDirectoryName(executingAssembly.Location))
                {
                    __result = Array.Empty<Type>();
                    return false;
                }

                //__result = __instance.GetExportedTypes();
                //__result = (Type[])internalGetTypes.Invoke(__instance, new object[] { false });
                // return true;

            Return:
                __result = _originalGetTypes.Invoke(__instance, false);

                return false;
            }
            catch (ReflectionTypeLoadException ex)
            {
                __result = ex.Types.Where(t => t != null).ToArray();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
