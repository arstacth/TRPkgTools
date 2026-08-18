using System;
using System.IO;
using System.IO.Compression;

namespace TRpkgTools
{
    internal static class PkgCrypto
    {
        public static byte[] DecryptHeader(byte[] header12)
        {
            byte[] xor = KeyProfiles.HeaderXor;
            byte[] output = new byte[header12.Length];
            for (int i = 0; i < header12.Length; i++)
                output[i] = (byte)(header12[i] ^ xor[i % 16]);
            return output;
        }

        public static byte[] EncryptHeader(byte[] header12)
        {
            return DecryptHeader(header12);
        }

        public static byte[] CryptContent(byte[] data, KeySet key, bool encrypt)
        {
            byte[] output = (byte[])data.Clone();
            int n = data.Length / 16 * 16;
            if (n > 0)
            {
                byte[] blocks = AesEcb(key.ContentAes, data, 0, n, encrypt);
                Buffer.BlockCopy(blocks, 0, output, 0, n);
            }
            int xorLen = key.ContentXor.Length;
            if (xorLen <= 0)
                xorLen = 16;
            for (int i = n; i < data.Length; i++)
                output[i] = (byte)(data[i] ^ key.ContentXor[i % xorLen]);
            return output;
        }

        public static byte[] DecryptContent(byte[] data, KeySet key)
        {
            return CryptContent(data, key, false);
        }

        public static byte[] EncryptContent(byte[] data, KeySet key)
        {
            return CryptContent(data, key, true);
        }

        public static bool LooksPlaintext(byte[] data)
        {
            return PlainScore(data) > 0;
        }

        public static int PlainScore(byte[] data)
        {
            if (data == null || data.Length < 4)
                return 0;
            if (data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                return (data.Length > 3 && data[3] == (byte)'<') ? 100 : 90;
            if (data[0] == 0xFF && data[1] == 0xFE)
                return 90;
            if (data[0] == 0xFE && data[1] == 0xFF)
                return 90;
            if (data[0] == 0x89 && data[1] == (byte)'P' && data[2] == (byte)'N' && data[3] == (byte)'G')
                return 90;
            if (data[0] == (byte)'D' && data[1] == (byte)'D' && data[2] == (byte)'S' && data[3] == (byte)' ')
                return 90;
            if (LooksLikeMarkup(data) || LooksLikeTextScript(data))
                return 80;
            return 0;
        }

        static bool IsNameChar(byte b)
        {
            return (b >= (byte)'A' && b <= (byte)'Z')
                || (b >= (byte)'a' && b <= (byte)'z')
                || (b >= (byte)'0' && b <= (byte)'9')
                || b == (byte)'_' || b == (byte)'-';
        }

        static bool LooksLikeMarkup(byte[] data)
        {
            if (data[0] != (byte)'<')
                return false;
            byte n1 = data[1];
            if (n1 == (byte)'?' || n1 == (byte)'!')
            {
                int ascii = 0;
                int m = Math.Min(8, data.Length);
                for (int k = 0; k < m; k++)
                {
                    byte b = data[k];
                    if (b == 9 || b == 10 || b == 13 || (b >= 32 && b < 127))
                        ascii++;
                }
                return ascii >= m * 3 / 4;
            }
            int i0 = 1;
            if (n1 == (byte)'/')
                i0 = 2;
            if (i0 >= data.Length || !IsNameChar(data[i0]) || (data[i0] >= (byte)'0' && data[i0] <= (byte)'9'))
                return false;
            int pos = i0 + 1;
            int letters = 1;
            while (pos < data.Length && IsNameChar(data[pos]))
            {
                letters++;
                pos++;
            }
            if (letters < 3 || pos >= data.Length)
                return false;
            byte end = data[pos];
            return end == (byte)'>' || end == (byte)'/' || end == (byte)' ' || end == 9 || end == 10 || end == 13;
        }

        static bool LooksLikeTextScript(byte[] data)
        {
            byte c = data[0];
            if (c != (byte)'#' && c != (byte)'/' && c != (byte)'[' && c != (byte)'{')
                return false;
            int m = Math.Min(16, data.Length);
            int ascii = 0;
            for (int i = 0; i < m; i++)
            {
                byte b = data[i];
                if (b == 9 || b == 10 || b == 13 || (b >= 32 && b < 127))
                    ascii++;
            }
            return ascii >= m * 7 / 8;
        }

        public static byte[] ZlibDecompress(byte[] data)
        {
            Exception last = null;
            int[] offsets = (data.Length >= 2 && (data[0] & 0x0F) == 8) ? new[] { 2, 0 } : new[] { 0, 2 };
            foreach (int offset in offsets)
            {
                if (offset >= data.Length)
                    continue;
                try
                {
                    using (var input = new MemoryStream(data, offset, data.Length - offset, false))
                    using (var deflate = new DeflateStream(input, CompressionMode.Decompress))
                    using (var output = new MemoryStream())
                    {
                        deflate.CopyTo(output);
                        return output.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    last = ex;
                }
            }
            throw new InvalidDataException("zlib decompress failed", last);
        }

        public static byte[] ZlibCompress(byte[] data)
        {
            byte[] deflate;
            using (var output = new MemoryStream())
            {
                using (var deflateStream = new DeflateStream(output, CompressionMode.Compress, true))
                    deflateStream.Write(data, 0, data.Length);
                deflate = output.ToArray();
            }
            uint adler = Adler32(data);
            byte[] zlib = new byte[2 + deflate.Length + 4];
            zlib[0] = 0x78;
            zlib[1] = 0x9C;
            Buffer.BlockCopy(deflate, 0, zlib, 2, deflate.Length);
            int t = zlib.Length - 4;
            zlib[t] = (byte)(adler >> 24);
            zlib[t + 1] = (byte)(adler >> 16);
            zlib[t + 2] = (byte)(adler >> 8);
            zlib[t + 3] = (byte)adler;
            return zlib;
        }

        static uint Adler32(byte[] data)
        {
            uint a = 1, b = 0;
            for (int i = 0; i < data.Length; i++)
            {
                a = (a + data[i]) % 65521;
                b = (b + a) % 65521;
            }
            return (b << 16) | a;
        }

        public static byte[] AesEcb(byte[] key, byte[] data, bool encrypt)
        {
            return AesEcb(key, data, 0, data.Length, encrypt);
        }

        public static byte[] AesEcb(byte[] key, byte[] data, int offset, int count, bool encrypt)
        {
            using (var aes = new System.Security.Cryptography.RijndaelManaged())
            {
                aes.Mode = System.Security.Cryptography.CipherMode.ECB;
                aes.Padding = System.Security.Cryptography.PaddingMode.None;
                aes.BlockSize = 128;
                aes.KeySize = key.Length * 8;
                aes.Key = key;
                using (var t = encrypt ? aes.CreateEncryptor() : aes.CreateDecryptor())
                    return t.TransformFinalBlock(data, offset, count);
            }
        }

        public static uint Crc32(byte[] data)
        {
            uint crc = 0xFFFFFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int b = 0; b < 8; b++)
                    crc = (crc >> 1) ^ (0xEDB88320u & (uint)-(crc & 1));
            }
            return crc ^ 0xFFFFFFFF;
        }
    }
}
