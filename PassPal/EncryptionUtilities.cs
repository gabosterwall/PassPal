﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PassPal
{
    public class EncryptionUtilities
    {
        // Method for generating a random secret key
        public byte[] CreateSecretKey()
        {
            const int keySize = 16;
            byte[] secretKey = new byte[keySize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretKey);
            }
            return secretKey;
        }

        // Same as above but for IV
        public byte[] CreateIV()
        {
            const int keySize = 16;
            byte[] randIV = new byte[keySize];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randIV);
            }
            return randIV;
        }

        // Method for encryption
        public byte[] EncryptVault(Dictionary<string, string> vault, byte[] vaultKey, byte[] iV)
        {
            byte[] encryptedVault;
            using (Aes aesAlgo = Aes.Create())
            {
                ICryptoTransform encryptor = aesAlgo.CreateEncryptor(vaultKey, iV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
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

        // Method for decryption
        public Dictionary<string, string> DecryptVault(byte[] encryptedVault, byte[] vaultKey, byte[] iV)
        {
            string simpleText = string.Empty;
            Dictionary<string, string> decryptedVault = new Dictionary<string, string>();
            try
            {
                using (Aes aesAlgo = Aes.Create())
                {
                    ICryptoTransform decryptor = aesAlgo.CreateDecryptor(vaultKey, iV);
                    using (MemoryStream ms = new MemoryStream(encryptedVault))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                simpleText = sr.ReadToEnd();
                            }
                        }
                    }
                }
                decryptedVault = JsonSerializer.Deserialize<Dictionary<string, string>>(simpleText) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
            }
            catch (CryptographicException)
            {
                throw new CryptographicException("\nError: decryption failed because of wrong secret key and/or wrong password, command aborted.");
            }
            catch (Exception)
            {
                throw new Exception("\nError: decryption failed because of unknown reasons.");
            }
            return decryptedVault;
        }
        
    }
}
