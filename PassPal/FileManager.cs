using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PassPal
{
    internal class FileManager
    {
        private void CreateClient(string _args1)
        {
            byte[] secretKey = EncryptionUtilities.CreateSecretKey();
            //string jsonSecKey = JsonSerializer.Serialize(secretKey);
            Dictionary<string, byte[]> client = new Dictionary<string, byte[]>();
            client.Add("secret", secretKey); //Hårdkoda OK..?
            string jsonClient = JsonSerializer.Serialize(client);
            File.WriteAllText(_args1, jsonClient);

            Console.WriteLine("New client successfully created!");

            string displayKey = Convert.ToBase64String(secretKey);
            Console.WriteLine($"Your secret key: {displayKey}");
        }

        //Metod för att läsa av den hemliga nyckeln från Client SOM ÄNTLIGEN VILL FUNKA
        private byte[] GetSecretKey(string _args1)
        {
            string jsonFromClient = File.ReadAllText(_args1);
            Dictionary<string, byte[]> keyFromClient = JsonSerializer.Deserialize<Dictionary<string, byte[]>>(jsonFromClient)!;  // '!' för att detta aldrig kommer vara NULL
            return keyFromClient["secret"]; //Kommer aldrig att ändras så hårdkoda OK..?
        }
        //Metod för att generera en vault key med Secret Key och Master Password
        private byte[] CreateVaultKey(string masterPass, byte[] secretKey)
        {
            int iterations = 10000; //antal iterationer, rekommenderad mängd
            byte[] vaultKey;
            int keySize = 32;

            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(masterPass, secretKey, iterations, HashAlgorithmName.SHA256))
            {
                vaultKey = deriveBytes.GetBytes(keySize);
            }
            return vaultKey;
        }

        //Metod för att skapa en ny server.json innehållandes ett nytt enkrypterat, tomt valv + IV
        public void Init(string args1, string args2, string pwd)
        {
            CreateClient(args1);    //Skapar client

            byte[] iV = EncryptionUtilities.CreateIV();     //Generar nytt IV för varje instans av en Server
            byte[] vaultKey = CreateVaultKey(pwd, GetSecretKey(args1)); //Skapar derived key (vaultkey) med Rfc2898DeriveBytes
            Dictionary<string, string> _emptyVault = new Dictionary<string, string>();
            byte[] encryptedVault = EncryptionUtilities.EncryptVault(_emptyVault, vaultKey, iV); //Enkryptering vars return matchar JsonObject:s egenskaper
            JsonVault jsonVault = new JsonVault(encryptedVault, iV);
            string json = JsonSerializer.Serialize(jsonVault);
            File.WriteAllText(args2, json);

            Console.WriteLine($"\nNew server successfully created!");
        }

        //Metod för [create]-kommando
        public void Create(string args1, string args2) // EJ KLAR FIXA SEN
        {
            if (File.Exists(args2))
            {
                try
                {
                    JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2))!; // '!' för att det ej kommer vara null
                    byte[] encryptedVault = jsonVault.Vault;
                    byte[] IV = jsonVault.IV;
                    byte[] inputSecKey = PasswordUtilities.InputSecretKey();
                    byte[] vaultKey = CreateVaultKey(PasswordUtilities.InputPassword(), inputSecKey);

                    Dictionary<string, string> decryptedVault = EncryptionUtilities.DecryptVault(encryptedVault, vaultKey, IV);
                    if (decryptedVault == null)
                        Console.WriteLine($"\nCommand aborted.");
                    else
                    {
                        Dictionary<string, byte[]> newClient = new Dictionary<string, byte[]>();
                        newClient.Add("secret", inputSecKey);
                        string json = JsonSerializer.Serialize(newClient);
                        File.WriteAllText(args1, json);
                        Console.WriteLine($"\nNew client successfully created for {args2}!");
                    }
                }
                catch (CryptographicException)
                {
                    Console.WriteLine($"\nError C.EX: Something went wrong, command aborted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\nError EX: Something went wrong, command aborted.");
                    Console.WriteLine(ex.Message);
                }

            }
            else
                Console.WriteLine($"\nError: {args2} not found.");

        }

        // Överlagrad metod för [get]-kommandot: första skriver ut alla properities i valv, andra ett specifikt prop och dess value
        public void Get(string args1, string args2)
        {
            if (File.Exists(args2))
            {
                byte[] vaultKey = CreateVaultKey(PasswordUtilities.InputPassword(), GetSecretKey(args1));
                JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2))!; // '!' för att det ej kommer vara null
                byte[] encryptedVault = jsonVault.Vault;
                byte[] IV = jsonVault.IV;

                Dictionary<string, string> decryptedVault = EncryptionUtilities.DecryptVault(encryptedVault, vaultKey, IV);
                if (decryptedVault == null)
                {
                    Console.WriteLine($"\nCommand aborted.");
                }
                else
                {
                    foreach (var item in decryptedVault)    // OBS! Detta är inspirerat från kodexemplet på studium
                    {
                        Console.WriteLine($"\n{item.Key}");
                    }
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }
        public void Get(string args1, string args2, string args3)
        {
            if (File.Exists(args2))
            {
                byte[] vaultKey = CreateVaultKey(PasswordUtilities.InputPassword(), GetSecretKey(args1));
                JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2))!; // '!' för att det ej kommer vara null
                byte[] _encryptedVault = jsonVault.Vault;
                byte[] _IV = jsonVault.IV;

                Dictionary<string, string> decryptedVault = EncryptionUtilities.DecryptVault(_encryptedVault, vaultKey, _IV);
                if (decryptedVault == null)
                    Console.WriteLine($"\nCommand aborted.");
                else
                {
                    if (decryptedVault.ContainsKey(args3))
                        Console.WriteLine($"\n{args3} : {decryptedVault[args3]}");
                    else
                        Console.WriteLine($"\nProperty '{args3}' could not be found.");
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }

        // Metod för [set]-kommando: 
        public void Set(string args1, string args2, string args3, bool isGenerated)
        {
            if (File.Exists(args2))
            {
                byte[] vaultKey = CreateVaultKey(PasswordUtilities.InputPassword(), GetSecretKey(args1));
                JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2))!; // '!' för att det ej kommer vara null
                byte[] encryptedVault = jsonVault.Vault;
                byte[] IV = jsonVault.IV;

                Dictionary<string, string>? decryptedVault = EncryptionUtilities.DecryptVault(encryptedVault, vaultKey, IV);
                if (decryptedVault == null)
                    Console.WriteLine($"\nCommand aborted.");
                else
                {
                    if (isGenerated == false)
                    {
                        decryptedVault.Add(args3, PasswordUtilities.AddPassword());
                        byte[] reEncryptedVault = EncryptionUtilities.EncryptVault(decryptedVault, vaultKey, IV);
                        JsonVault updatedVault = new JsonVault(reEncryptedVault, IV);
                        string json = JsonSerializer.Serialize(updatedVault);
                        File.WriteAllText(args2, json);
                        Console.WriteLine($"\nNew password successfully stored!");
                    }
                    else
                    {
                        decryptedVault.Add(args3, PasswordUtilities.GenerateRandomPassword());
                        byte[] reEncryptedVault = EncryptionUtilities.EncryptVault(decryptedVault, vaultKey, IV);
                        JsonVault updatedVault = new JsonVault(reEncryptedVault, IV);
                        string json = JsonSerializer.Serialize(updatedVault);
                        File.WriteAllText(args2, json);
                        Console.WriteLine($"\nNew password successfully stored!");
                    }
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }
        public void Delete(string args1, string args2, string args3)
        {
            if (File.Exists(args2))
            {
                byte[] vaultKey = CreateVaultKey(PasswordUtilities.InputPassword(), GetSecretKey(args1));
                JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2))!; // '!' för att det ej kommer vara null
                byte[] encryptedVault = jsonVault.Vault;
                byte[] IV = jsonVault.IV;

                Dictionary<string, string>? decryptedVault = EncryptionUtilities.DecryptVault(encryptedVault, vaultKey, IV);
                if (decryptedVault == null)
                    Console.WriteLine($"\nCommand aborted.");
                else
                {
                    if (decryptedVault.ContainsKey(args3))
                    {
                        decryptedVault.Remove(args3);
                        byte[] reEncryptedVault = EncryptionUtilities.EncryptVault(decryptedVault, vaultKey, IV);
                        JsonVault updatedVault = new JsonVault(reEncryptedVault, IV);
                        string json = JsonSerializer.Serialize(updatedVault);
                        File.WriteAllText(args2, json);
                        Console.WriteLine($"{args3} successfully removed from the vault!");
                    }
                    else
                        Console.WriteLine($"\nError: {args3} does not exist in the vault, command aborted. ");
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }
        public void Secret(string args1)
        {
            string jsonFromClient = File.ReadAllText(args1);
            Dictionary<string, byte[]> keyFromClient = JsonSerializer.Deserialize<Dictionary<string, byte[]>>(jsonFromClient)!;  // '!' för att detta aldrig kommer vara NULL
            string displayKey = Convert.ToBase64String(keyFromClient["secret"]); //Kommer aldrig att ändras så hårdkoda OK..?
            Console.WriteLine($"\nSecret key for {args1}: {displayKey}");
        }
        
    }
}
