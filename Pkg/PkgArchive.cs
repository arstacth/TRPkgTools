using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TRpkgTools
{
    internal sealed class PkgEntry
    {
        public string Path;
        public int PartCount;
        public uint BodyOffset;
        public int EncryptType;
    }

    internal sealed class PkgHeader
    {
        public string Signature;
        public uint Version;
        public uint Checksum;
        public uint DataBegin;
        public int FileCount;
    }

    internal static class PkgArchive
    {
        public static PkgHeader ReadHeader(Stream pkg)
        {
            pkg.Position = 0;
            byte[] sigRaw = new byte[12];
            pkg.Read(sigRaw, 0, 12);
            string sig;
            if (Encoding.ASCII.GetString(sigRaw).StartsWith("PackageFile", StringComparison.OrdinalIgnoreCase))
                sig = "PackageFile";
            else
                sig = Encoding.ASCII.GetString(PkgCrypto.DecryptHeader(sigRaw));

            var br = new BinaryReader(pkg);
            uint ver = br.ReadUInt32();
            uint checksum = br.ReadUInt32();
            uint dataBegin = br.ReadUInt32();

            pkg.Position = dataBegin;
            br.ReadUInt32();
            int fileCount = br.ReadInt32();
            br.ReadUInt32();

            return new PkgHeader
            {
                Signature = sig,
                Version = ver,
                Checksum = checksum,
                DataBegin = dataBegin,
                FileCount = fileCount
            };
        }

        public static bool IsValidSignature(string sig)
        {
            if (!string.IsNullOrEmpty(sig) && sig.StartsWith("PackageFile", StringComparison.OrdinalIgnoreCase))
                return true;
            foreach (string s in KeyProfiles.Signatures)
                if (sig == s)
                    return true;
            return false;
        }

        public static List<PkgEntry> ReadAllEntries(Stream pkg, PkgHeader header)
        {
            pkg.Position = header.DataBegin;
            var br = new BinaryReader(pkg);
            br.ReadUInt32();
            br.ReadInt32();
            br.ReadUInt32();

            Encoding euc = Encoding.GetEncoding("euc-kr");
            var list = new List<PkgEntry>(header.FileCount);
            for (int n = 0; n < header.FileCount; n++)
            {
                int packed = br.ReadInt32();
                byte[] blob = br.ReadBytes(packed);
                byte[] entry = PkgCrypto.ZlibDecompress(blob);
                int z = Array.IndexOf(entry, (byte)0);
                string path = euc.GetString(entry, 0, z < 0 ? entry.Length : z);
                list.Add(new PkgEntry
                {
                    Path = path.Replace('/', '\\'),
                    PartCount = BitConverter.ToInt32(entry, 0x410),
                    BodyOffset = BitConverter.ToUInt32(entry, 0x414)
                });
            }
            return list;
        }

        public static byte[] ReadFile(Stream pkg, PkgEntry entry, KeySet key)
        {
            pkg.Position = entry.BodyOffset;
            var br = new BinaryReader(pkg);
            using (var ms = new MemoryStream())
            {
                int lastType = 0;
                for (int i = 0; i < entry.PartCount; i++)
                {
                    br.ReadUInt32();
                    br.ReadUInt32();
                    int size = br.ReadInt32();
                    br.ReadUInt32();
                    int encryptType = br.ReadInt32();
                    lastType = encryptType;
                    if (size <= 0)
                        continue;
                    byte[] data = br.ReadBytes(size);
                    if ((encryptType & 1) != 0)
                        data = PkgCrypto.ZlibDecompress(data);
                    if ((encryptType & 2) != 0)
                        data = PkgCrypto.DecryptContent(data, key);
                    ms.Write(data, 0, data.Length);
                }
                entry.EncryptType = lastType;
                return ms.ToArray();
            }
        }

        public static KeySet DetectKey(Stream pkg, PkgHeader header)
        {
            List<PkgEntry> entries = ReadAllEntries(pkg, header);
            bool anyAes = false;
            int[] hits = new int[KeyProfiles.All.Length];
            int sampled = 0;
            foreach (PkgEntry entry in entries)
            {
                if (entry.PartCount <= 0)
                    continue;
                pkg.Position = entry.BodyOffset;
                var br = new BinaryReader(pkg);
                br.ReadUInt32();
                br.ReadUInt32();
                int size = br.ReadInt32();
                br.ReadUInt32();
                int encryptType = br.ReadInt32();
                if (size <= 0)
                    continue;
                byte[] data = br.ReadBytes(size);
                if ((encryptType & 1) != 0)
                {
                    try
                    {
                        data = PkgCrypto.ZlibDecompress(data);
                    }
                    catch
                    {
                        continue;
                    }
                }
                if ((encryptType & 2) == 0)
                    continue;
                anyAes = true;
                sampled++;
                for (int k = 0; k < KeyProfiles.All.Length; k++)
                {
                    byte[] pt = PkgCrypto.DecryptContent(data, KeyProfiles.All[k]);
                    int score = PkgCrypto.PlainScore(pt);
                    if (score >= 90)
                        return KeyProfiles.All[k];
                    if (score >= 80)
                        hits[k]++;
                }
                if (sampled >= 16)
                    break;
            }
            int bestHits = 0;
            int bestIndex = -1;
            for (int k = 0; k < hits.Length; k++)
            {
                if (hits[k] > bestHits)
                {
                    bestHits = hits[k];
                    bestIndex = k;
                }
            }
            if (bestHits >= 3)
                return KeyProfiles.All[bestIndex];
            if (!anyAes)
                return KeyProfiles.Plain2012;
            return null;
        }
    }
}
