# TRPkgTools

Unpack and repack Tales Runner `.pkg` files.

Drop a pkg, browse to one, or use Unpack / Repack. The key is detected automatically from the files inside the pkg.

<img width="520" height="352" alt="pkg_tools" src="https://github.com/user-attachments/assets/8544d879-617f-41c5-9917-c2037bef1788" />


## Features

- **Unpack** — decrypt and extract files from a `.pkg`
- **Repack** — pack files back using `filelist_[name].txt`
- **Auto key detect** — tries built-in keys and picks the one that decrypts as plaintext
- **Unpack at PKG Location** — off = extract next to `TRPkgTools.exe`; on = extract in the same folder as the `.pkg`
- **DEBUG** — `tr4.pkg` only. Decrypts one test file (`script\abusedata.txt`, or `script\configuration.xml` if that is missing) and opens it in Notepad so you can check the key
- **Old 2012 pkgs** — `PackageFile` header, zlib only, no AES (`# key=2012`)

After unpack you get `filelist_[name].txt` with `# key=` / `# ascii=` so repack can use the same key.

## Add a new key

If a newer client changes the pkg key, add it in `TRpkgTools\Pkg\KeyProfiles.cs` inside `All`.

Most new clients use a 32-char ASCII hex from `trgame.exe` (TRKeyExtractor can copy this line):

```csharp
Ascii("2027XX", "YOUR32CHARASCIIHEXKEYHERE00000000"),
```

Older / custom clients that already have derived AES + XOR bytes:

```csharp
Raw("Name", "AESHEX...", "XORHEX..."),
```

Rebuild after editing. Unpack will auto-detect the new key if it matches the pkg.
