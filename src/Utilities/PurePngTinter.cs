namespace SdtdServerKit.Utilities
{
    /// <summary>
    /// PNG 染色器
    /// </summary>
    internal static class PurePngTinter
    {
        /// <summary>
        /// 解码 PNG
        /// </summary>
        public static bool TryTint(byte[] inputBytes, int r, int g, int b, bool skipFullyTransparent, out byte[]? result)
        {
            result = null;
            try
            {
                if (PurePng.TryDecode(inputBytes, out int width, out int height, out byte[]? rgba) == false || rgba == null)
                {
                    return false;
                }

                float fr = r / 255f;
                float fg = g / 255f;
                float fb = b / 255f;
                int len = rgba.Length;
                for (int i = 0; i < len; i += 4)
                {
                    if (skipFullyTransparent && rgba[i + 3] == 0)
                    {
                        continue;
                    }
                    rgba[i] = (byte)(rgba[i] * fr);
                    rgba[i + 1] = (byte)(rgba[i + 1] * fg);
                    rgba[i + 2] = (byte)(rgba[i + 2] * fb);
                }

                result = PurePng.Encode(width, height, rgba);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
