namespace SdtdServerKit.Utilities
{
    /// <summary>
    /// 图标染色工具
    /// </summary>
    internal static class IconTinter
    {
        /// <summary>缓存目录名（位于 Mod 的 Assets 目录下）</summary>
        private const string CacheDirName = "IconTintCache";

        private static string? _cacheDir;
        private static readonly object _initLock = new object();

        /// <summary>
        /// 读取 PNG 文件并染色。先查磁盘缓存，未命中则染色并落盘。
        /// </summary>
        public static byte[] TintPng(string iconPath, int r, int g, int b, bool skipFullyTransparent = false)
        {
            string key = BuildKey(iconPath, r, g, b, skipFullyTransparent);
            string? dir = EnsureCacheDir();
            string? cacheFile = dir == null ? null : Path.Combine(dir, key + ".png");

            if (cacheFile != null && File.Exists(cacheFile))
            {
                try
                {
                    return File.ReadAllBytes(cacheFile);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[IconTinter] 读取磁盘缓存失败，回退重新染色：{ex.Message}");
                }
            }

            byte[] inputBytes = File.ReadAllBytes(iconPath);
            if (PurePngTinter.TryTint(inputBytes, r, g, b, skipFullyTransparent, out byte[]? tinted) == false || tinted == null)
            {
                throw new InvalidDataException(
                    "无法对图标进行染色，可能是不支持的 PNG 格式（仅支持 8 位 RGB/RGBA、非隔行）：" + iconPath);
            }

            if (cacheFile != null)
            {
                try
                {
                    File.WriteAllBytes(cacheFile, tinted);
                }
                catch (Exception ex)
                {
                    Log.Warning($"[IconTinter] 写入磁盘缓存失败：{ex.Message}");
                }
            }

            return tinted;
        }

        /// <summary>
        /// 缓存 key：icon 文件名（不含扩展名） + RGB 十六进制 + skip 标志。
        /// </summary>
        private static string BuildKey(string iconPath, int r, int g, int b, bool skipFullyTransparent)
        {
            string fileName = Path.GetFileNameWithoutExtension(iconPath);
            return string.Concat(
                fileName,
                "__",
                r.ToString("X2"),
                g.ToString("X2"),
                b.ToString("X2"),
                skipFullyTransparent ? "_s" : "_n");
        }

        private static string? EnsureCacheDir()
        {
            if (_cacheDir != null)
            {
                return _cacheDir;
            }

            lock (_initLock)
            {
                if (_cacheDir != null)
                {
                    return _cacheDir;
                }

                try
                {
                    string dir = Path.Combine(ModApi.ModInstance.Path, "Assets", CacheDirName);
                    Directory.CreateDirectory(dir);
                    _cacheDir = dir;
                    return _cacheDir;
                }
                catch (Exception ex)
                {
                    Log.Warning($"[IconTinter] 创建磁盘缓存目录失败，每次请求都会重新染色：{ex.Message}");
                    return null;
                }
            }
        }
    }
}
