using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace TRpkgTools
{
    internal sealed class PackItem
    {
        public string RelPath;
        public string FullPath;
    }

    internal static class PkgPack
    {
        const int BodyStart = 0x20;
        const int EntrySize = 0x51C;
        const int ChunkSize = 102400;

        public static void Write(string pkgPath, KeySet key, IList<PackItem> files, uint checksum, Action<int, int, string> progress)
        {
            Encoding euc = Encoding.GetEncoding("euc-kr");
            string tmp = pkgPath + ".tmp";
            using (var fs = new FileStream(tmp, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            {
                // Official header is 0x20: sig, ver, checksum, dataBegin, 0x1C, 0x51C.
                fs.Write(new byte[BodyStart], 0, BodyStart);

                int n = files.Count;
                var offsets = new uint[n];
                var plains = new int[n];
                var bodyBytes = new int[n];
                var parts = new int[n];
                var hashes = new byte[n][];

                for (int i = 0; i < n; i++)
                {
                    PackItem item = files[i];
                    if (progress != null)
                        progress(i + 1, n, item.RelPath);
                    byte[] plain = File.ReadAllBytes(item.FullPath);
                    plains[i] = plain.Length;
                    offsets[i] = (uint)fs.Position;
                    using (var md5 = MD5.Create())
                    {
                        int partCount = 0;
                        int totalBody = 0;
                        int pos = 0;
                        if (plain.Length == 0)
                        {
                            WritePart(fs, md5, key, new byte[0], 0);
                            partCount = 1;
                            totalBody = 20;
                        }
                        else
                        {
                            while (pos < plain.Length)
                            {
                                int len = Math.Min(ChunkSize, plain.Length - pos);
                                byte[] chunk = new byte[len];
                                Buffer.BlockCopy(plain, pos, chunk, 0, len);
                                int stored = WritePart(fs, md5, key, chunk, partCount);
                                totalBody += stored + 20;
                                partCount++;
                                pos += len;
                            }
                        }
                        md5.TransformFinalBlock(new byte[0], 0, 0);
                        hashes[i] = md5.Hash;
                        parts[i] = partCount;
                        bodyBytes[i] = totalBody;
                    }
                }

                uint tablePos = (uint)fs.Position;
                WriteU32(fs, 0);
                WriteU32(fs, (uint)n);
                long unk2Pos = fs.Position;
                WriteU32(fs, 0);

                uint tableBytes = 0;
                for (int i = 0; i < n; i++)
                {
                    byte[] entry = BuildEntry(euc, files[i].RelPath, plains[i], bodyBytes[i], parts[i], offsets[i], hashes[i]);
                    byte[] z = PkgCrypto.ZlibCompress(entry);
                    WriteU32(fs, (uint)z.Length);
                    fs.Write(z, 0, z.Length);
                    tableBytes += (uint)z.Length;
                }

                fs.Position = unk2Pos;
                WriteU32(fs, tableBytes);

                fs.Position = 0;
                byte[] sig = Encoding.ASCII.GetBytes("ACAC35E5-4B7");
                byte[] encSig = PkgCrypto.EncryptHeader(sig);
                fs.Write(encSig, 0, encSig.Length);
                WriteU32(fs, 2);
                WriteU32(fs, checksum);
                WriteU32(fs, tablePos);
                WriteU32(fs, 0x1C);
                WriteU32(fs, EntrySize);
            }

            if (File.Exists(pkgPath))
                File.Delete(pkgPath);
            File.Move(tmp, pkgPath);
        }

        static int WritePart(Stream fs, MD5 md5, KeySet key, byte[] plain, int seq)
        {
            byte[] enc = PkgCrypto.EncryptContent(plain, key);
            byte[] z = PkgCrypto.ZlibCompress(enc);
            byte[] stored;
            int encryptType;
            if (z.Length + 16 < enc.Length)
            {
                stored = z;
                encryptType = 3;
            }
            else
            {
                stored = enc;
                encryptType = 2;
            }
            WriteU32(fs, (uint)seq);
            WriteU32(fs, (uint)plain.Length);
            WriteU32(fs, (uint)stored.Length);
            WriteU32(fs, PkgCrypto.Crc32(stored));
            WriteU32(fs, (uint)encryptType);
            fs.Write(stored, 0, stored.Length);
            if (stored.Length > 0)
                md5.TransformBlock(stored, 0, stored.Length, stored, 0);
            return stored.Length;
        }

        static byte[] BuildEntry(Encoding euc, string rel, int plainLen, int bodyBytes, int partCount, uint bodyOffset, byte[] md5)
        {
            byte[] entry = new byte[EntrySize];
            string path = rel.Replace('/', '\\');
            byte[] name = euc.GetBytes(path);
            int n = Math.Min(name.Length, 0x3FF);
            Buffer.BlockCopy(name, 0, entry, 0, n);
            WriteInt(entry, 0x404, plainLen);
            WriteInt(entry, 0x408, bodyBytes);
            WriteInt(entry, 0x410, partCount);
            BitConverter.GetBytes(bodyOffset).CopyTo(entry, 0x414);
            WriteInt(entry, 0x508, 0x01000000);
            Buffer.BlockCopy(md5, 0, entry, 0x50C, 16);
            return entry;
        }

        static void WriteInt(byte[] buf, int offset, int value)
        {
            BitConverter.GetBytes(value).CopyTo(buf, offset);
        }

        static void WriteU32(Stream fs, uint value)
        {
            fs.Write(BitConverter.GetBytes(value), 0, 4);
        }

        public static string PackageNameFromFilelist(string listPath)
        {
            string name = Path.GetFileName(listPath);
            if (name == null)
                return null;
            if (name.StartsWith("filelist_", StringComparison.OrdinalIgnoreCase) &&
                name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                return name.Substring("filelist_".Length, name.Length - "filelist_".Length - 4);
            }
            return null;
        }

        public static List<PackItem> ReadFilelist(string listPath, string root)
        {
            var files = new List<PackItem>();
            string listDir = Path.GetDirectoryName(Path.GetFullPath(listPath)) ?? root;
            string listName = Path.GetFileName(listPath);
            foreach (string line in File.ReadAllLines(listPath, Encoding.UTF8))
            {
                string s = (line ?? "").Trim().Trim('"');
                if (s.Length == 0 || s[0] == '#')
                    continue;
                s = s.Replace('/', '\\');
                if (string.Equals(Path.GetFileName(s), listName, StringComparison.OrdinalIgnoreCase))
                    continue;
                if (s.EndsWith(".pkg", StringComparison.OrdinalIgnoreCase))
                    continue;

                string full;
                string rel;
                if (Path.IsPathRooted(s))
                {
                    full = Path.GetFullPath(s);
                    rel = RelativeTo(root, full) ?? RelativeTo(listDir, full);
                    if (rel == null)
                        rel = Path.GetFileName(full);
                }
                else
                {
                    string underRoot = Path.GetFullPath(Path.Combine(root, s));
                    string underList = Path.GetFullPath(Path.Combine(listDir, s));
                    if (File.Exists(underRoot))
                    {
                        full = underRoot;
                        rel = s;
                    }
                    else if (File.Exists(underList))
                    {
                        full = underList;
                        rel = s;
                    }
                    else
                    {
                        full = underRoot;
                        rel = s;
                    }
                }

                if (!File.Exists(full))
                    continue;
                files.Add(new PackItem { RelPath = rel.Replace('/', '\\'), FullPath = full });
            }
            return files;
        }

        public static string RelativeTo(string root, string full)
        {
            if (string.IsNullOrEmpty(root) || string.IsNullOrEmpty(full))
                return null;
            root = Path.GetFullPath(root);
            full = Path.GetFullPath(full);
            if (!root.EndsWith("\\"))
                root += "\\";
            if (full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return full.Substring(root.Length);
            return null;
        }

        public static void SavePackInfo(string listPath, KeySet key, string sourcePkg, uint checksum)
        {
            try
            {
                if (key != null && !string.IsNullOrEmpty(key.AsciiHex))
                    File.WriteAllText(listPath + ":pkgkey", key.AsciiHex);
                if (!string.IsNullOrEmpty(sourcePkg))
                    File.WriteAllText(listPath + ":srcpkg", sourcePkg);
                File.WriteAllText(listPath + ":csum", checksum.ToString("X"));
            }
            catch
            {
            }
        }

        public static void LoadPackInfo(string listPath, out KeySet key, out string sourcePkg, out uint checksum)
        {
            key = null;
            sourcePkg = null;
            checksum = 0;
            try
            {
                foreach (string raw in File.ReadAllLines(listPath, Encoding.UTF8))
                {
                    string line = (raw ?? "").Trim();
                    if (line.Length < 6 || line[0] != '#')
                        continue;
                    int eq = line.IndexOf('=');
                    if (eq < 0)
                        continue;
                    string name = line.Substring(1, eq - 1).Trim().ToLowerInvariant();
                    string val = line.Substring(eq + 1).Trim();
                    if (name == "ascii" && val.Length > 0)
                        key = KeyProfiles.FindByAscii(val);
                    else if (name == "key" && key == null && val.Length > 0)
                        key = KeyProfiles.FindByName(val);
                    else if (name == "csum")
                        uint.TryParse(val, System.Globalization.NumberStyles.HexNumber, null, out checksum);
                    else if (name == "pkg" && val.Length > 0)
                        sourcePkg = val;
                }
            }
            catch
            {
            }
            if (key != null)
                return;
            try
            {
                string text = File.ReadAllText(listPath + ":pkgkey").Trim();
                if (text.Length > 0)
                    key = KeyProfiles.FindByAscii(text);
            }
            catch
            {
            }
            try
            {
                string text = File.ReadAllText(listPath + ":srcpkg").Trim();
                if (text.Length > 0)
                    sourcePkg = text;
            }
            catch
            {
            }
            try
            {
                string text = File.ReadAllText(listPath + ":csum").Trim();
                uint parsed;
                if (uint.TryParse(text, System.Globalization.NumberStyles.HexNumber, null, out parsed))
                    checksum = parsed;
            }
            catch
            {
            }
        }
    }
}
