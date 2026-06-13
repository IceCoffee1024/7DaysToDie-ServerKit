//using HarmonyLib;
//using UnityEngine;

//namespace SdtdServerKit.HarmonyPatchers.PerformanceTuning
//{
//    [HarmonyPatch]
//    internal class AstarManagerPatcher
//    {
//        [HarmonyPrefix]
//        [HarmonyPatch(typeof(AstarManager), nameof(AstarManager.Init))]
//        public static bool Init(GameObject obj)
//        {
//            if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty")
//            {
//                return false;
//            }

//            CustomLogger.Info("AstarManager Init");
//            obj.AddComponent<AstarManager>();
//            new SdtdServerKit.HarmonyPatchers.PerformanceTuning.ASPPathFinderThread().StartWorkerThreads();
//            return false;
//        }
//    }
//}