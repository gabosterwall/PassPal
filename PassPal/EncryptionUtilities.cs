using System;
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
        public byte[] CreateSecretKey()
        {
            byte[] secretKey = new byte[16]; //Rätt storlek? FRÅGA UNDER HANDLEDNING
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(secretKey);
            }
            return secretKey;
        }

        public byte[] CreateIV()
        {
            byte[] randIV = new byte[16]; //Samma som ovan

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randIV);
            }
            return randIV;
        }

        public byte[] EncryptVault(Dictionary<string, string> vault, byte[] vaultKey, byte[] iV)
        {
            byte[] encryptedVault;

            using (Aes aesAlgo = Aes.Create())
            {
                //aesAlgo.Key = vaultKey;
                //aesAlgo.IV = iV;

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aesAlgo.CreateEncryptor(vaultKey, iV), CryptoStreamMode.Write))
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
            Console.WriteLine("\nEncryption successfull!");
            return encryptedVault;
        }
        public Dictionary<string, string> DecryptVault(byte[] encryptedVault, byte[] vaultKey, byte[] iV)
        {
            string jsonVault = string.Empty;
            Dictionary<string, string> decryptedVault = new Dictionary<string, string>();
            try
            {
                using (Aes aesAlgo = Aes.Create())
                {
                    //aesAlgo.Key = vaultKey;
                    //aesAlgo.IV = iV;

                    using (MemoryStream ms = new MemoryStream(encryptedVault))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aesAlgo.CreateDecryptor(vaultKey, iV), CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs))
                            {
                                jsonVault = sr.ReadToEnd();
                                //decryptedVault = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonVault) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
                                //while ((jsonVault = sr.ReadLine()) != null)
                                //{
                                //    string[] entry = jsonVault.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                                //    if (entry.Length == 2)
                                //    {
                                //        decryptedVault[entry[0]] = entry[1];
                                //    }
                                //}
                            }
                        }
                    }
                }
                decryptedVault = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonVault) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
            }
            catch (CryptographicException)
            {
                throw new CryptographicException("\nError: decryption failed because of wrong secret key and/or wrong password, command aborted.");
            }
            catch (Exception)
            {
                throw new Exception("\nError: decryption failed because of unknown reasons.");
            }
            Console.WriteLine("\nDecryption successfull!");
            return decryptedVault;

        }
        public Dictionary<string, string> DecryptVault(string args2, byte[] vaultKey)
        {
            JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
            byte[] encryptedVault = jsonVault.Vault;
            byte[] IV = jsonVault.IV;

            string vaultToText = string.Empty;
            Dictionary<string, string> decryptedVault = new Dictionary<string, string>();
            try
            {
                using (Aes aesAlgo = Aes.Create())
                {
                    aesAlgo.Key = vaultKey;
                    aesAlgo.IV = IV;

                    using (MemoryStream ms = new MemoryStream(encryptedVault))
                    {
                        using (CryptoStream cs = new CryptoStream(ms, aesAlgo.CreateDecryptor(), CryptoStreamMode.Read))
                        {
                            using (StreamReader sr = new StreamReader(cs)) //samma som ovan
                            {
                                vaultToText = sr.ReadToEnd();
                            }
                        }
                    }
                }
                decryptedVault = JsonSerializer.Deserialize<Dictionary<string, string>>(vaultToText) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
            }
            catch (CryptographicException)
            {
                throw new Exception("\nError: decryption failed because of wrong vault key or wrong IV.");
            }
            catch (Exception)
            {
                throw new Exception("\nError: decryption failed because of unknown reasons.");
            }
            return decryptedVault;

        }
    }
}
