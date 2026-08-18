using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TRpkgTools
{
    internal static class PkgJobs
    {
        public static string ExeDir()
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            if (string.IsNullOrEmpty(dir))
                dir = Path.GetDirectoryName(System.Windows.Forms.Application.ExecutablePath);
            return Path.GetFullPath(dir);
        }

        public static string Unpack(string pkgPath, string outDir, Action<int, int, string> progress)
        {
            string baseName = Path.GetFileNameWithoutExtension(pkgPath);
            using (var fs = File.OpenRead(pkgPath))
            {
                PkgHeader header = PkgArchive.ReadHeader(fs);
                if (!PkgArchive.IsValidSignature(header.Signature))
                    throw new InvalidDataException("Not a Tales Runner pkg.");
                KeySet key = PkgArchive.DetectKey(fs, header);
                if (key == null)
                    throw new InvalidDataException("No matching decrypt key.");

                var entries = PkgArchive.ReadAllEntries(fs, header);
                int errors = 0;
                Directory.CreateDirectory(outDir);
                string listPath = Path.Combine(outDir, "filelist_" + baseName + ".txt");
                var list = new StringBuilder();
                list.AppendLine("# key=" + key.Name);
                if (!string.IsNullOrEmpty(key.AsciiHex))
                    list.AppendLine("# ascii=" + key.AsciiHex);
                for (int i = 0; i < entries.Count; i++)
                {
                    PkgEntry entry = entries[i];
                    if (progress != null)
                        progress(i + 1, entries.Count, entry.Path);
                    try
                    {
                        byte[] data = PkgArchive.ReadFile(fs, entry, key);
                        string rel = (entry.Path ?? "").Replace('/', '\\').TrimStart('\\');
                        if (string.IsNullOrEmpty(rel))
                            continue;
                        string dest = Path.GetFullPath(Path.Combine(outDir, rel));
                        if (PkgPack.RelativeTo(outDir, dest) == null)
                        {
                            errors++;
                            continue;
                        }
                        string dir = Path.GetDirectoryName(dest);
                        if (!string.IsNullOrEmpty(dir))
                            Directory.CreateDirectory(dir);
                        File.WriteAllBytes(dest, data);
                        list.AppendLine(dest);
                    }
                    catch
                    {
                        errors++;
                    }
                }

                File.WriteAllText(listPath, list.ToString(), new UTF8Encoding(false));
                PkgPack.SavePackInfo(listPath, key, pkgPath, header.Checksum);
                if (errors != 0)
                    return "Unpacked with " + errors + " errors.  " + outDir;
                return "Unpacked " + entries.Count + " files.  " + outDir;
            }
        }

        public static string UnpackDebug(string pkgPath, string outDir, Action<int, int, string> progress, out string openedPath)
        {
            openedPath = null;
            string name = Path.GetFileName(pkgPath);
            if (!string.Equals(name, "tr4.pkg", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Select a tr4.pkg.");

            using (var fs = File.OpenRead(pkgPath))
            {
                PkgHeader header = PkgArchive.ReadHeader(fs);
                if (!PkgArchive.IsValidSignature(header.Signature))
                    throw new InvalidDataException("Not a Tales Runner pkg.");
                KeySet key = PkgArchive.DetectKey(fs, header);
                if (key == null)
                    throw new InvalidDataException("No matching decrypt key.");

                var entries = PkgArchive.ReadAllEntries(fs, header);
                PkgEntry hit = FindDebugEntry(entries);
                if (hit == null)
                    throw new InvalidDataException("script\\abusedata.txt / script\\configuration.xml not in tr4.pkg.");

                if (progress != null)
                    progress(1, 1, hit.Path);
                byte[] data = PkgArchive.ReadFile(fs, hit, key);
                Directory.CreateDirectory(outDir);
                string rel = (hit.Path ?? "").Replace('/', '\\').TrimStart('\\');
                string dest = Path.GetFullPath(Path.Combine(outDir, rel));
                if (PkgPack.RelativeTo(outDir, dest) == null)
                    throw new InvalidDataException("Bad unpack path.");
                string dir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrEmpty(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllBytes(dest, data);
                openedPath = dest;
                return "DEBUG key=" + key.Name + "  " + dest;
            }
        }

        static PkgEntry FindDebugEntry(List<PkgEntry> entries)
        {
            string[] paths =
            {
                "script\\abusedata.txt",
                "script\\configuration.xml"
            };
            foreach (string want in paths)
            {
                foreach (PkgEntry entry in entries)
                {
                    string rel = (entry.Path ?? "").Replace('/', '\\').TrimStart('\\');
                    if (string.Equals(rel, want, StringComparison.OrdinalIgnoreCase))
                        return entry;
                }
            }
            foreach (string want in paths)
            {
                string tail = want.Substring(want.LastIndexOf('\\') + 1);
                foreach (PkgEntry entry in entries)
                {
                    string rel = (entry.Path ?? "").Replace('/', '\\').TrimStart('\\');
                    if (rel.EndsWith("\\" + tail, StringComparison.OrdinalIgnoreCase)
                        || string.Equals(rel, tail, StringComparison.OrdinalIgnoreCase))
                        return entry;
                }
            }
            return null;
        }

        public static string Repack(string listPath, string exeDir, Action<int, int, string> progress)
        {
            string name = PkgPack.PackageNameFromFilelist(listPath);
            if (string.IsNullOrEmpty(name))
                throw new InvalidDataException("Repack needs filelist_[name].txt.");
            var files = PkgPack.ReadFilelist(listPath, exeDir);
            if (files.Count == 0)
                throw new InvalidDataException("No files in filelist.");

            KeySet key;
            string sourcePkg;
            uint checksum;
            PkgPack.LoadPackInfo(listPath, out key, out sourcePkg, out checksum);

            if (key == null && !string.IsNullOrEmpty(sourcePkg) && File.Exists(sourcePkg))
                key = DetectFromPkg(sourcePkg, ref checksum);

            string outPkg = Path.Combine(exeDir, name + ".pkg");
            if (key == null && File.Exists(outPkg))
                key = DetectFromPkg(outPkg, ref checksum);
            if (key == null)
                key = KeyProfiles.All[0];

            PkgPack.Write(outPkg, key, files, checksum, progress);
            return "Packed " + files.Count + " files.";
        }

        public static KeySet DetectFromPkg(string pkgPath, ref uint checksum)
        {
            try
            {
                using (var fs = File.OpenRead(pkgPath))
                {
                    PkgHeader header = PkgArchive.ReadHeader(fs);
                    if (!PkgArchive.IsValidSignature(header.Signature))
                        return null;
                    checksum = header.Checksum;
                    return PkgArchive.DetectKey(fs, header);
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
