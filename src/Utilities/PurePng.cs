using System.IO.Compression;
using System.Text;

namespace SdtdServerKit.Utilities
{
    /// <summary>
    /// PNG 编解码器
    /// </summary>
    internal static class PurePng
    {
        private static readonly byte[] PngSignature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };


        public static byte[] Encode(int width, int height, byte[] rgba)
        {
            using var output = new MemoryStream();

            output.Write(PngSignature, 0, PngSignature.Length);

            var ihdr = new byte[13];
            WriteInt32BE(ihdr, 0, width);
            WriteInt32BE(ihdr, 4, height);
            ihdr[8] = 8;   
            ihdr[9] = 6;   
            ihdr[10] = 0;  
            ihdr[11] = 0;  
            ihdr[12] = 0;  
            WriteChunk(output, "IHDR", ihdr, 0, ihdr.Length);

            int rowBytes = width * 4;
            var filtered = new byte[(rowBytes + 1) * height];
            for (int y = 0; y < height; y++)
            {
                int dst = y * (rowBytes + 1);
                filtered[dst] = 0; 
                Buffer.BlockCopy(rgba, y * rowBytes, filtered, dst + 1, rowBytes);
            }

            byte[] zlibCompressed = ZlibCompress(filtered);
            WriteChunk(output, "IDAT", zlibCompressed, 0, zlibCompressed.Length);

            WriteChunk(output, "IEND", Array.Empty<byte>(), 0, 0);

            return output.ToArray();
        }

        /// <summary>
        /// 解码 PNG 到 RGBA8 像素缓冲区。
        /// </summary>
        public static bool TryDecode(byte[] data, out int width, out int height, out byte[]? rgba)
        {
            width = 0;
            height = 0;
            rgba = null;

            if (data.Length < 8) return false;
            for (int i = 0; i < 8; i++)
            {
                if (data[i] != PngSignature[i]) return false;
            }

            int pos = 8;
            int bitDepth = 0;
            int colorType = -1;
            byte interlace = 0;
            using var idatBuffer = new MemoryStream();

            while (pos + 8 <= data.Length)
            {
                int chunkLen = ReadInt32BE(data, pos);
                pos += 4;
                if (chunkLen < 0 || pos + 4 + chunkLen + 4 > data.Length) return false;

                string chunkType = Encoding.ASCII.GetString(data, pos, 4);
                pos += 4;

                if (chunkType == "IHDR")
                {
                    if (chunkLen < 13) return false;
                    width = ReadInt32BE(data, pos);
                    height = ReadInt32BE(data, pos + 4);
                    bitDepth = data[pos + 8];
                    colorType = data[pos + 9];
                    byte compression = data[pos + 10];
                    byte filter = data[pos + 11];
                    interlace = data[pos + 12];

                    if (width <= 0 || height <= 0) return false;
                    if (compression != 0 || filter != 0 || interlace != 0) return false;
                    if (bitDepth != 8) return false;
                    if (colorType != 2 && colorType != 6) return false;
                }
                else if (chunkType == "IDAT")
                {
                    idatBuffer.Write(data, pos, chunkLen);
                }
                else if (chunkType == "IEND")
                {
                    break;
                }

                pos += chunkLen + 4; 
            }

            if (colorType < 0) return false;

            byte[] compressed = idatBuffer.ToArray();
            if (compressed.Length < 6) return false;

            byte[] raw;
            using (var inputStream = new MemoryStream(compressed, 2, compressed.Length - 6))
            using (var deflate = new DeflateStream(inputStream, CompressionMode.Decompress))
            using (var output = new MemoryStream())
            {
                deflate.CopyTo(output);
                raw = output.ToArray();
            }

            int srcChannels = colorType == 6 ? 4 : 3;
            int rowBytes = width * srcChannels;
            int expectedSize = (rowBytes + 1) * height;
            if (raw.Length != expectedSize) return false;

            byte[] prevRow = new byte[rowBytes];
            byte[] currRow = new byte[rowBytes];
            rgba = new byte[width * height * 4];

            for (int y = 0; y < height; y++)
            {
                int rowStart = y * (rowBytes + 1);
                byte filterType = raw[rowStart];
                Buffer.BlockCopy(raw, rowStart + 1, currRow, 0, rowBytes);

                switch (filterType)
                {
                    case 0: 
                        break;
                    case 1: 
                        for (int x = srcChannels; x < rowBytes; x++)
                        {
                            currRow[x] = (byte)(currRow[x] + currRow[x - srcChannels]);
                        }
                        break;
                    case 2: 
                        for (int x = 0; x < rowBytes; x++)
                        {
                            currRow[x] = (byte)(currRow[x] + prevRow[x]);
                        }
                        break;
                    case 3: 
                        for (int x = 0; x < rowBytes; x++)
                        {
                            int left = x >= srcChannels ? currRow[x - srcChannels] : 0;
                            int up = prevRow[x];
                            currRow[x] = (byte)(currRow[x] + (left + up) / 2);
                        }
                        break;
                    case 4: 
                        for (int x = 0; x < rowBytes; x++)
                        {
                            int left = x >= srcChannels ? currRow[x - srcChannels] : 0;
                            int up = prevRow[x];
                            int upLeft = x >= srcChannels ? prevRow[x - srcChannels] : 0;
                            currRow[x] = (byte)(currRow[x] + PaethPredictor(left, up, upLeft));
                        }
                        break;
                    default:
                        return false;
                }

                int dstBase = y * width * 4;
                if (colorType == 6)
                {
                    Buffer.BlockCopy(currRow, 0, rgba, dstBase, rowBytes);
                }
                else 
                {
                    int dst = dstBase;
                    int src = 0;
                    for (int x = 0; x < width; x++)
                    {
                        rgba[dst++] = currRow[src++];
                        rgba[dst++] = currRow[src++];
                        rgba[dst++] = currRow[src++];
                        rgba[dst++] = 255;
                    }
                }

                (prevRow, currRow) = (currRow, prevRow);
            }

            return true;
        }

        private static int PaethPredictor(int a, int b, int c)
        {
            int p = a + b - c;
            int pa = Math.Abs(p - a);
            int pb = Math.Abs(p - b);
            int pc = Math.Abs(p - c);
            if (pa <= pb && pa <= pc) return a;
            if (pb <= pc) return b;
            return c;
        }

        private static byte[] ZlibCompress(byte[] data)
        {
            using var ms = new MemoryStream();
            ms.WriteByte(0x78);
            ms.WriteByte(0x9C);
            using (var deflate = new DeflateStream(ms, CompressionLevel.Fastest, leaveOpen: true))
            {
                deflate.Write(data, 0, data.Length);
            }
            uint adler = Adler32(data);
            ms.WriteByte((byte)(adler >> 24));
            ms.WriteByte((byte)(adler >> 16));
            ms.WriteByte((byte)(adler >> 8));
            ms.WriteByte((byte)adler);
            return ms.ToArray();
        }

        private static void WriteChunk(Stream output, string type, byte[] data, int offset, int length)
        {
            var lenBytes = new byte[4];
            WriteInt32BE(lenBytes, 0, length);
            output.Write(lenBytes, 0, 4);

            var typeBytes = Encoding.ASCII.GetBytes(type);
            output.Write(typeBytes, 0, 4);

            if (length > 0)
            {
                output.Write(data, offset, length);
            }

            uint crc = Crc32Begin();
            crc = Crc32Update(crc, typeBytes, 0, 4);
            crc = Crc32Update(crc, data, offset, length);
            crc = Crc32End(crc);
            var crcBytes = new byte[4];
            WriteInt32BE(crcBytes, 0, (int)crc);
            output.Write(crcBytes, 0, 4);
        }

        private static int ReadInt32BE(byte[] buf, int offset)
        {
            return (buf[offset] << 24) | (buf[offset + 1] << 16) | (buf[offset + 2] << 8) | buf[offset + 3];
        }

        private static void WriteInt32BE(byte[] buf, int offset, int value)
        {
            buf[offset] = (byte)((value >> 24) & 0xFF);
            buf[offset + 1] = (byte)((value >> 16) & 0xFF);
            buf[offset + 2] = (byte)((value >> 8) & 0xFF);
            buf[offset + 3] = (byte)(value & 0xFF);
        }

        private static readonly uint[] _crcTable = BuildCrcTable();

        private static uint[] BuildCrcTable()
        {
            var t = new uint[256];
            for (uint n = 0; n < 256; n++)
            {
                uint c = n;
                for (int k = 0; k < 8; k++)
                {
                    c = (c & 1) != 0 ? (0xEDB88320u ^ (c >> 1)) : (c >> 1);
                }
                t[n] = c;
            }
            return t;
        }

        private static uint Crc32Begin() => 0xFFFFFFFFu;

        private static uint Crc32Update(uint crc, byte[] data, int offset, int length)
        {
            for (int i = 0; i < length; i++)
            {
                crc = _crcTable[(crc ^ data[offset + i]) & 0xFF] ^ (crc >> 8);
            }
            return crc;
        }

        private static uint Crc32End(uint crc) => crc ^ 0xFFFFFFFFu;

        // 用于 zlib 校验
        private static uint Adler32(byte[] data)
        {
            const uint MOD = 65521;
            uint a = 1, b = 0;
            int len = data.Length;
            for (int i = 0; i < len; i++)
            {
                a = (a + data[i]) % MOD;
                b = (b + a) % MOD;
            }
            return (b << 16) | a;
        }
    }
}
