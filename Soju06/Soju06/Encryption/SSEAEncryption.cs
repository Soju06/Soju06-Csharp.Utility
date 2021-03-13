/* ========= Soju06 SSEA Encryption Utility =========
 * NAMESPACE: Soju06.Encryption
 * LICENSE: MIT
 * Copyright by Soju06
 * ========= Soju06 SSEA Encryption Utility ========= */
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Soju06.Encryption {
    public class SSEAEncryption {
        public static byte[] CmprsEncrypt(byte[] data, string key) {
            var odata = AES256Encrypt(data, key);
            using (var stream = new MemoryStream())
            using (var ds = new DeflateStream(stream, CompressionMode.Compress)) {
                ds.Write(odata, 0, odata.Length);
                ds.Flush(); ds.Close();
                odata = stream.ToArray();
            }
            InvertBytes(ref odata);
            return odata;
        }

        public static byte[] CmprsDecrypt(byte[] data, string key) {
            InvertBytes(ref data);
            using (var stream = new MemoryStream(data))
            using (var output = new MemoryStream())
            using (var ds = new DeflateStream(stream, CompressionMode.Decompress)) {
                ds.CopyTo(output);
                data = output.ToArray();
            }
            return AES256Decrypt(data, key);
        }

        public static byte[] Encrypt(byte[] data, string key) {
            var Key = MD5Hash(key);
            var odata = AES256Encrypt(data, Key);
            InvertBytes(ref odata);
            return odata;
        }

        public static byte[] Decrypt(byte[] data, string key) {
            var Key = MD5Hash(key);
            InvertBytes(ref data);
            return AES256Decrypt(data, Key);
        }

        private static void InvertBytes(ref byte[] bytes) {
            for (int i = 0; i < bytes.Length; i++)
                if(i != 0 && i % 2f != 0)
                    bytes[i] = (byte)(byte.MaxValue - bytes[i]);
        }

        private static string MD5Hash(string text) {
            byte[] result = new MD5CryptoServiceProvider()
                .ComputeHash(Encoding.ASCII.GetBytes(text));
            var sb = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
                sb.Append(result[i].ToString("x2"));
            return sb.ToString();
        }

        private static byte[] AES256Encrypt(byte[] data, string pw) {
            var Key = MD5Hash(pw);
            using (var password = new PasswordDeriveBytes(Key,
                Encoding.ASCII.GetBytes(Key.Length.ToString())))
            using (var ms = new MemoryStream())
            using (var crypto = new RijndaelManaged()
                .CreateEncryptor(password.GetBytes(32), password.GetBytes(16)))
            using (var cs = new CryptoStream(ms, crypto, CryptoStreamMode.Write)) {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
                return ms.ToArray();
            }
        }

        private static byte[] AES256Decrypt(byte[] data, string pw) {
            var Key = MD5Hash(pw);
            using (var password = new PasswordDeriveBytes(Key, 
                Encoding.ASCII.GetBytes(Key.Length.ToString())))
            using (var ms = new MemoryStream(data))
            using (var cryptoStream = new CryptoStream(ms, 
                new RijndaelManaged().CreateDecryptor(password.GetBytes(32), 
                password.GetBytes(16)), CryptoStreamMode.Read)) {
                var odata = new byte[data.Length];
                Array.Resize(ref odata, cryptoStream.Read(odata, 0, odata.Length));
                return odata;
            }
        }
    }
}
