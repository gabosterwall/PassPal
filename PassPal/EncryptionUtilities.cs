using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PassPal
{
    internal static class EncryptionUtilities
    {
        public static byte[] CreateSecretKey()
        {
            byte[] secretKey = new byte[16]; //Rätt storlek? FRÅGA UNDER HANDLEDNING
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretKey);
            }
            return secretKey;
        }

        public static byte[] CreateIV()
        {
            byte[] randIV = new byte[16]; //Samma som ovan

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randIV);
            }
            return randIV;
        }

        public static byte[] EncryptVault(Dictionary<string, string> vault, byte[] vaultKey, byte[] iV)
        {
            byte[] encryptedVault;

            using (Aes aesAlgo = Aes.Create())
            {
                aesAlgo.Key = vaultKey;
                aesAlgo.IV = iV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aesAlgo.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            string jsonVault = JsonSerializer.Serialize(vault);
                            sw.Write(jsonVault);
                        }
                        encryptedVault = ms.ToArray();
                    }
                }
            }
            return encryptedVault;
        }

        public static Dictionary<string, string> DecryptVault(byte[] encryptedVault, byte[] vaultKey, byte[] iV)
        {
            string jsonVault = string.Empty;
            Dictionary<string, string>? decryptedVault = new Dictionary<string, string>();
            try
            {
                using (Aes aesAlgo = Aes.Create())
                {
                    aesAlgo.Key = vaultKey;
                    aesAlgo.IV = iV;

                    using (MemoryStream ms = new MemoryStream(encryptedVault))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aesAlgo.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                jsonVault = sr.ReadToEnd();
                            }
                        }
                    }
                }
                decryptedVault = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonVault); //kommer ej vara null
            }
            catch (CryptographicException)
            {
                Console.WriteLine($"\n Decryption failed: incorrect vault key and/or IV.");
                return null!;
            }
            catch (Exception)
            {
                Console.WriteLine($"\n Error occured: possible wrong password used.");
                return null!;
            }
            return decryptedVault!;
        }
    }
}
