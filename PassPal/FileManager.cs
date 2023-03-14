using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace PassPal
{
    public class FileManager : EncryptionUtilities
    {
        private void CreateClient(string args1)
        {
            const string keyName = "secret"; //för att inte hårdkoda...
            byte[] secretKey = CreateSecretKey();
            Dictionary<string, byte[]> client = new Dictionary<string, byte[]>
            {
                { keyName, secretKey }
            };
            string jsonClient = JsonSerializer.Serialize(client);
            File.WriteAllText(args1, jsonClient);
            Console.WriteLine($"\nNew client '{args1}' successfully created!");
            string displayKey = Convert.ToBase64String(secretKey);
            Console.WriteLine($"\nYour secret key: {displayKey}");
        }

        //Metod för att läsa av den hemliga nyckeln från Client SOM ÄNTLIGEN VILL FUNKA
        private byte[] GetSecretKey(string args1)
        {
            const string keyName = "secret";
            string jsonFromClient = File.ReadAllText(args1);
            Dictionary<string, byte[]> keyFromClient = JsonSerializer.Deserialize<Dictionary<string, byte[]>>(jsonFromClient)!;  // '!' för att detta aldrig kommer vara NULL
            return keyFromClient[keyName]; 
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
            CreateClient(args1); //Skapar client

            byte[] secretKey = GetSecretKey(args1);
            byte[] iV = CreateIV(); //Generar nytt IV för varje instans av en Server
            byte[] vaultKey = CreateVaultKey(pwd, secretKey); //Skapar derived key (vaultkey) med Rfc2898DeriveBytes

            Dictionary<string, string> emptyVault = new Dictionary<string, string>();
            byte[] encryptedVault = EncryptVault(emptyVault, vaultKey, iV); //Enkryptering vars return matchar JsonObject:s egenskaper

            JsonVault jsonVault = new JsonVault(encryptedVault, iV);
            string json = JsonSerializer.Serialize(jsonVault);
            File.WriteAllText(args2, json);

            if(File.Exists(args2))
                Console.WriteLine($"\nNew server '{args2}' successfully created!");
            else
                Console.WriteLine("\nError: server could not be created.");
        }

        //Metod för [create]-kommando
        public void Create(string args1, string args2, string pwd, string secretKey) // EJ KLAR FIXA SEN
        {
            if (File.Exists(args2))
            {
                try
                {
                    const string keyName = "secret";
                    byte[] key = Convert.FromBase64String(secretKey);
                    byte[] vaultKey = CreateVaultKey(pwd, key);

                    JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
                    byte[] encryptedVault = jsonVault.Vault;
                    byte[] IV = jsonVault.IV;
                    Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);

                    if(decryptedVault != null)
                    {
                        Dictionary<string, byte[]> newClient = new Dictionary<string, byte[]>
                        {
                            { keyName, key }
                        };
                        string json = JsonSerializer.Serialize(newClient);
                        File.WriteAllText(args1, json);
                        if (File.Exists(args1))
                            Console.WriteLine($"\nNew client '{args1}' successfully created!");
                        else
                            Console.WriteLine($"\nError: {args1} could not be created.");
                    }
                    else
                        Console.WriteLine("\nError: decryption failed, wrong key and/or password used.");
                    

                }
                catch (CryptographicException ce)
                {
                    Console.WriteLine($"\n{ce.Message}");
                }
                catch(FormatException)
                {
                    Console.WriteLine("\nError: wrong secret key, command aborted.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}");
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }

        //Överlagrad metod för [get]-kommandot: första skriver ut alla properities i valv, andra ett specifikt prop och dess value
        public void Get(string args1, string args2, string pwd) // Kolla under handledning
        {
            if (File.Exists(args2))
            {
                try
                {
                    byte[] secretKey = GetSecretKey(args1);
                    byte[] vaultKey = CreateVaultKey(pwd, secretKey);

                    JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted"); //Är detta OK?
                    byte[] encryptedVault = jsonVault.Vault;
                    byte[] IV = jsonVault.IV;

                    Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                    foreach(var item in decryptedVault)
                    {
                        Console.WriteLine($"\n{item.Key}");
                    }
                }
                catch(JsonException)
                {
                    Console.WriteLine($"\nError: invalid JSON-format in {args2}, command aborted.");
                }
                catch(CryptographicException)
                {
                    Console.WriteLine($"\nError: decryption failed because of wrong 'vault key' or 'IV', command aborted.");
                }
                catch(Exception)
                {
                    Console.WriteLine($"\nUnkown error occurred, command aborted.");
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }
        public void Get(string args1, string args2, string pwd, string args3) // Kolla under handledning
        {
            if (File.Exists(args2))
            {
                try
                {
                    byte[] secretKey = GetSecretKey(args1);
                    byte[] vaultKey = CreateVaultKey(pwd, secretKey);

                    JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
                    byte[] encryptedVault = jsonVault.Vault;
                    byte[] IV = jsonVault.IV;

                    Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                    if (decryptedVault.ContainsKey(args3))
                    {
                        Console.WriteLine($"\n{args3} : {decryptedVault[args3]}");
                    }
                    else
                        Console.WriteLine($"\nProperty '{args3}' could not be found.");
                }
                catch (JsonException)
                {
                    Console.WriteLine($"\nError: invalid JSON-format in {args2}, command aborted.");
                }
                catch (CryptographicException)
                {
                    Console.WriteLine($"\nError: decryption failed because of wrong 'vault key' or 'IV', command aborted.");
                }
                catch (Exception)
                {
                    Console.WriteLine($"\nUnkown error occurred, command aborted.");
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }

        // Metod för [set]-kommando: 
        public void Set(string args1, string args2, string args3, string pwd, string passToAdd)
        {
            if (File.Exists(args2))
            {
                try
                {
                    byte[] vaultKey = CreateVaultKey(pwd, GetSecretKey(args1));

                    JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
                    byte[] encryptedVault = jsonVault.Vault;
                    byte[] IV = jsonVault.IV;

                    Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                    decryptedVault.Add(args3, passToAdd);

                    byte[] reEncryptedVault = EncryptVault(decryptedVault, vaultKey, IV);
                    JsonVault updatedVault = new JsonVault(reEncryptedVault, IV);
                    string json = JsonSerializer.Serialize(updatedVault);

                    File.WriteAllText(args2, json);
                    Console.WriteLine($"\nNew password successfully stored!");
                }
                catch (JsonException)
                {
                    Console.WriteLine($"\nError: invalid JSON-format in {args2}, command aborted.");
                }
                catch (CryptographicException)
                {
                    Console.WriteLine($"\nError: decryption failed because of wrong 'vault key' or 'IV', command aborted.");
                }
                catch (Exception)
                {
                    Console.WriteLine($"\nUnkown error occurred, command aborted.");
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }
        public void Set(string args1, string args2, string args3, string args4, string pwd, string passToAdd)
        {
            if (File.Exists(args2))
            {
                try
                {
                    if (args4 == "-g" || args4 == "--generate")
                    {
                        byte[] vaultKey = CreateVaultKey(pwd, GetSecretKey(args1));

                        JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
                        byte[] encryptedVault = jsonVault.Vault;
                        byte[] IV = jsonVault.IV;

                        Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                        decryptedVault.Add(args3, passToAdd);

                        byte[] reEncryptedVault = EncryptVault(decryptedVault, vaultKey, IV);
                        JsonVault updatedVault = new JsonVault(reEncryptedVault, IV);
                        string json = JsonSerializer.Serialize(updatedVault);

                        File.WriteAllText(args2, json);
                        Console.WriteLine($"\nNew password successfully stored!");                    }
                    else
                        Console.WriteLine($"\nError: {args4} not a valid flag, command aborted.");
                }
                catch (JsonException)
                {
                    Console.WriteLine($"\nError: invalid JSON-format in {args2}, command aborted.");
                }
                catch (CryptographicException)
                {
                    Console.WriteLine($"\nError: decryption failed because of wrong 'vault key' or 'IV', command aborted.");
                }
                catch (Exception)
                {
                    Console.WriteLine($"\nUnkown error occurred, command aborted.");
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }

        public void Delete(string args1, string args2, string args3, string pwd)
        {
            if (File.Exists(args2))
            {
                try
                {
                    byte[] vaultKey = CreateVaultKey(pwd, GetSecretKey(args1));
                    JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
                    byte[] encryptedVault = jsonVault.Vault;
                    byte[] IV = jsonVault.IV;

                    Dictionary<string, string>? decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                    if(decryptedVault.ContainsKey(args3))
                    {
                        decryptedVault.Remove(args3);
                        byte[] reEncryptedVault = EncryptVault(decryptedVault, vaultKey, IV);
                        JsonVault updatedVault = new JsonVault(reEncryptedVault, IV);
                        string json = JsonSerializer.Serialize(updatedVault);
                        File.WriteAllText(args2, json);
                        Console.WriteLine($"{args3} successfully removed from the vault!");
                    }
                    else
                        Console.WriteLine($"\nError: {args3} does not exist in the vault, command aborted. ");
                }
                catch (JsonException)
                {
                    Console.WriteLine($"\nError: invalid JSON-format in {args2}, command aborted.");
                }
                catch (CryptographicException)
                {
                    Console.WriteLine($"\nError: decryption failed because of wrong 'vault key' or 'IV', command aborted.");
                }
                catch (Exception)
                {
                    Console.WriteLine($"\nUnkown error occurred, command aborted.");
                }
            }
            else
                Console.WriteLine($"\nError: {args2} not found.");
        }
        public void Secret(string filePath)
        {
            byte[] secretKey = GetSecretKey(filePath);
            string displayKey = Convert.ToBase64String(secretKey);
            Console.WriteLine($"\nSecret key for '{filePath}' : {displayKey}");
        }
        
    }
}
