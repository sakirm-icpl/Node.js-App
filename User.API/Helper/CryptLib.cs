// Decompiled with JetBrains decompiler
// Type: com.pakhee.common.CryptLib
// Assembly: ETMS.CDAL, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 3E3B975D-93EF-4A76-910F-0EA6FCB33563
// Assembly location: C:\Users\chetan.mali\Desktop\SBIL_09August\bin\ETMS.CDAL.dll

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace com.pakhee.common
{
    public static class CryptLib
    {
        private static UTF8Encoding _enc = new UTF8Encoding();
        private static RijndaelManaged _rcipher = new RijndaelManaged();
        private static byte[] _key = new byte[32];
        private static byte[] _iv = new byte[CryptLib._rcipher.BlockSize / 8];
        private static byte[] _ivBytes = new byte[16];
        private static readonly char[] CharacterMatrixForRandomIVStringGeneration = new char[64]
    {
      'A',
      'B',
      'C',
      'D',
      'E',
      'F',
      'G',
      'H',
      'I',
      'J',
      'K',
      'L',
      'M',
      'N',
      'O',
      'P',
      'Q',
      'R',
      'S',
      'T',
      'U',
      'V',
      'W',
      'X',
      'Y',
      'Z',
      'a',
      'b',
      'c',
      'd',
      'e',
      'f',
      'g',
      'h',
      'i',
      'j',
      'k',
      'l',
      'm',
      'n',
      'o',
      'p',
      'q',
      'r',
      's',
      't',
      'u',
      'v',
      'w',
      'x',
      'y',
      'z',
      '0',
      '1',
      '2',
      '3',
      '4',
      '5',
      '6',
      '7',
      '8',
      '9',
      '-',
      '_'
    };
        private static byte[] _pwd;

        static CryptLib()
        {
            CryptLib._rcipher.Mode = CipherMode.CBC;
            CryptLib._rcipher.Padding = PaddingMode.PKCS7;
            CryptLib._rcipher.KeySize = 128;
            CryptLib._rcipher.BlockSize = 128;
        }

        internal static string GenerateRandomIV(int length)
        {
            char[] chArray = new char[length];
            byte[] data = new byte[length];
            using (RNGCryptoServiceProvider cryptoServiceProvider = new RNGCryptoServiceProvider())
                cryptoServiceProvider.GetBytes(data);
            for (int index1 = 0; index1 < chArray.Length; ++index1)
            {
                int index2 = (int)data[index1] % CryptLib.CharacterMatrixForRandomIVStringGeneration.Length;
                chArray[index1] = CryptLib.CharacterMatrixForRandomIVStringGeneration[index2];
            }
            return new string(chArray);
        }

        private static string encryptDecrypt(string _inputText, string _encryptionKey, CryptLib.EncryptMode _mode, string _initVector)
        {
            CryptLib._pwd = Encoding.UTF8.GetBytes(_encryptionKey);
            CryptLib._ivBytes = Encoding.UTF8.GetBytes(_initVector);
            byte[] buffer = Convert.FromBase64String(_inputText);
            string str = string.Empty;
            using (Rijndael rijndael = Rijndael.Create())
            {
                rijndael.Key = CryptLib._pwd;
                rijndael.IV = CryptLib._ivBytes;
                ICryptoTransform decryptor = rijndael.CreateDecryptor(rijndael.Key, rijndael.IV);
                using (MemoryStream memoryStream = new MemoryStream(buffer))
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))
                            str = streamReader.ReadToEnd();
                    }
                }
                return str;
            }
        }

        public static string encrypt(string _plainText, string _key, string _initVector)
        {
            return CryptLib.encryptDecrypt(_plainText, _key, CryptLib.EncryptMode.ENCRYPT, _initVector);
        }

        public static string decrypt(string _encryptedText, string _key, string _initVector)
        {
            return CryptLib.encryptDecrypt(_encryptedText, _key, CryptLib.EncryptMode.DECRYPT, _initVector);
        }

        public static string getHashSha256(string text, int length)
        {
            byte[] hash = new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(text));
            string empty = string.Empty;
            foreach (byte num in hash)
                empty += string.Format("{0:x2}", (object)num);
            if (length > empty.Length)
                return empty;
            return empty.Substring(0, length);
        }

        private static string MD5Hash(string text)
        {
            MD5 md5 = (MD5)new MD5CryptoServiceProvider();
            md5.ComputeHash(Encoding.ASCII.GetBytes(text));
            byte[] hash = md5.Hash;
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < hash.Length; ++index)
                stringBuilder.Append(hash[index].ToString("x2"));
            Console.WriteLine("md5 hash of they key=" + stringBuilder.ToString());
            return stringBuilder.ToString();
        }

        private enum EncryptMode
        {
            ENCRYPT,
            DECRYPT,
        }
    }
}
