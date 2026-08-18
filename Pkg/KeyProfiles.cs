using System;

namespace TRpkgTools
{
    internal sealed class KeySet
    {
        public string Name;
        public string AsciiHex;
        public byte[] HeaderXor;
        public byte[] ContentAes;
        public byte[] ContentXor;
    }

    internal static class KeyProfiles
    {
        public static readonly byte[] HeaderSeed = Hex("0D68076F0A09076C65730D756E0A650D");

        public static readonly string[] Signatures = { "ACAC35E5-4B7", "ACAC3E55-4B7" };

        public static readonly KeySet[] All = Build(
            Ascii("2026TH", "B211FAB3FCCE4C07A7FA6EF9CD93959B"),
            Ascii("2026KR", "CECA72E7ACDB485C8DF0AC16A2EA6D92"),
            Ascii("2026HK", "B7E392F1A0D84B6C9E25F71D3A8C6B4E"),
            Raw("Mango",
                "5E7DCED850E7A568BF1A04923B1D73D35F59A5CD37AE8BF5AC32128E8DA267D0",
                "10468AE20A2AF6A19A1468E1F96B6F321FD324E98693B51F97B19C50CE45F4C2"),
            Ascii("2026CN", "A73FF15DBE914FCC880037FB32E4D314"),
            Raw("2",
                "FDD715CBBEBFA5FFEF9EED97CE96D30F4CDCA01DAF5FCFA2D8B15808B9B6C10A",
                "2044B2A363C747884D1E2F1290393C8E"),
            Raw("2016HK",
                "0D68076F0A09076C65730D756E0A650D",
                "055BCB64FBC2CEB4778B1BB8E9B59CC6")
        );

        public static readonly KeySet Plain2012 = new KeySet
        {
            Name = "2012",
            AsciiHex = "",
            HeaderXor = HeaderXor,
            ContentAes = HeaderSeed,
            ContentXor = HeaderXor
        };

        public static byte[] HeaderXor
        {
            get { return PkgCrypto.AesEcb(HeaderSeed, HeaderSeed, true); }
        }

        public static KeySet FindByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;
            string want = name.Replace(" ", "");
            foreach (KeySet key in All)
            {
                if (string.Equals(key.Name.Replace(" ", ""), want, StringComparison.OrdinalIgnoreCase))
                    return key;
            }
            return null;
        }

        public static KeySet FindByAscii(string asciiHex)
        {
            if (string.IsNullOrEmpty(asciiHex))
                return null;
            foreach (KeySet key in All)
            {
                if (string.Equals(key.AsciiHex, asciiHex, StringComparison.OrdinalIgnoreCase))
                    return key;
            }
            return FromAscii("cached", asciiHex);
        }

        public static KeySet FromAscii(string name, string asciiHex)
        {
            return Ascii(name, asciiHex);
        }

        static KeySet Ascii(string name, string asciiHex)
        {
            byte[] ascii = System.Text.Encoding.ASCII.GetBytes(asciiHex);
            byte[] aes = PkgCrypto.AesEcb(HeaderSeed, ascii, true);
            byte[] first = new byte[16];
            Buffer.BlockCopy(aes, 0, first, 0, 16);
            return new KeySet
            {
                Name = name,
                AsciiHex = asciiHex,
                HeaderXor = HeaderXor,
                ContentAes = aes,
                ContentXor = PkgCrypto.AesEcb(aes, first, true)
            };
        }

        static KeySet Raw(string name, string contentAesHex, string contentXorHex)
        {
            return new KeySet
            {
                Name = name,
                AsciiHex = "",
                HeaderXor = HeaderXor,
                ContentAes = Hex(contentAesHex),
                ContentXor = Hex(contentXorHex)
            };
        }

        static KeySet[] Build(params KeySet[] sets)
        {
            return sets;
        }

        public static byte[] Hex(string hex)
        {
            if (hex.Length % 2 != 0)
                throw new ArgumentException("hex length");
            byte[] b = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length / 2; i++)
                b[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            return b;
        }
    }
}
