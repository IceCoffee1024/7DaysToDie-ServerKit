using HarmonyLib;
using SdtdServerKit.Functions;
using UnityEngine;

namespace SdtdServerKit.HarmonyPatchers
{
    /// <summary>
    /// 抑制完整地图后台渲染期间的 “Chunk (..) unavailable during POI reset...” 刷屏日志。
    /// </summary>
    [HarmonyPatch(typeof(Debug), nameof(Debug.LogError), new[] { typeof(object) })]
    internal static class DebugLogErrorSuppressor
    {
        private const string SuppressKeyword = "unavailable during POI reset";


        [HarmonyPrefix]
        public static bool Before_LogError(object message)
        {
            if (!FullMapGenerateRenderer.SuppressPoiResetLog)
            {
                return true;
            }

            if (message != null)
            {
                string text = message.ToString();
                if (text != null && text.Contains(SuppressKeyword))
                {
                    return false; // 跳过原方法，抑制输出
                }
            }

            return true;
        }
    }
}
