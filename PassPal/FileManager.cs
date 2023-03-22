using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace PassPal
{
    public class FileManager : EncryptionUtilities
    {
        private void CreateClient(string args1)
        {
            const string keyName = "secret";
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

        // Method for retrieving secret key from given client
        private byte[] GetSecretKey(string args1)
        {
            const string keyName = "secret";
            string jsonFromClient = File.ReadAllText(args1);

            // Switched to ?? throw new ArgumentNullException instead of assigning the null-forgiving '!' at each deserialization; safer for handling potenital errors
            Dictionary<string, byte[]> keyFromClient = JsonSerializer.Deserialize<Dictionary<string, byte[]>>(jsonFromClient) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
            return keyFromClient[keyName]; 
        }

        // Method for generating the vault key from master password and secret key
        private byte[] CreateVaultKey(string masterPass, byte[] secretKey)
        {
            int iterations = 10000; //iterations, recommended amount
            byte[] vaultKey;
            int keySize = 16;

            using (Rfc2898DeriveBytes deriveBytes = new Rfc2898DeriveBytes(masterPass, secretKey, iterations, HashAlgorithmName.SHA256))
            {
                vaultKey = deriveBytes.GetBytes(keySize);
            }
            return vaultKey;
        }

        // Init-command method
        public void Init(string args1, string args2, string pwd)
        {
            CreateClient(args1);                                                        // Creates a new client

            byte[] secretKey = GetSecretKey(args1);
            byte[] iV = CreateIV();                                                     // Generates a new IV for each new instance of a server
            byte[] vaultKey = CreateVaultKey(pwd, secretKey);                           // Generates a specific new vault key with master password and secret key with the Rfc2898DeriveBytes-class

            Dictionary<string, string> emptyVault = new Dictionary<string, string>();
            byte[] encryptedVault = EncryptVault(emptyVault, vaultKey, iV);             // Encryption that returns the type that matches the properties of a JsonVault-object

            JsonVault jsonVault = new JsonVault(encryptedVault, iV);                    // Assign the properties, and then serialize and write them into a new server-file
            string json = JsonSerializer.Serialize(jsonVault);
            File.WriteAllText(args2, json);

            if(File.Exists(args2))
                Console.WriteLine($"\nNew server '{args2}' successfully created!");
            else
                Console.WriteLine("\nError: server could not be created.");
        }

        // Create-command method
        public void Create(string args1, string args2, string pwd/*, string secretKey*/) 
        {
            if (File.Exists(args2))
            {
                try
                {
                    const string keyName = "secret";
                    Console.WriteLine("\nEnter your secret key: ");
                    byte[] key = Convert.FromBase64String(PasswordUtilities.UserKeyInput());
                    byte[] vaultKey = CreateVaultKey(pwd, key);

                    JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
                    byte[] encryptedVault = jsonVault.Vault;
                    byte[] IV = jsonVault.IV;
                    
                    Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                    
                    if(decryptedVault != null)
                    {
                        Console.WriteLine("\nDecryption successfull!");

                        Dictionary<string, byte[]> newClient = new Dictionary<string, byte[]>
                        {
                            { keyName, key }
                        };
                        string json = JsonSerializer.Serialize(newClient);
                        File.WriteAllText(args1, json);
                    }
                    else
                        Console.WriteLine("\nDecryption failed.");

                    if (File.Exists(args1))
                        Console.WriteLine($"\nNew client '{args1}' successfully created!");
                    else
                        Console.WriteLine($"\nError: {args1} could not be created.");
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

        // Get-command overloaded method; one for listing all existing props and one for listing specific prop's password
        public void Get(string args1, string args2, string pwd)
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
                    foreach (var item in decryptedVault)
                    {
                        Console.WriteLine(item.Key);
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
        public void Get(string args1, string args2, string pwd, string args3)
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
                        Console.WriteLine(decryptedVault[args3]);
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

        // Set-command overloaded method; one that takes user input of new password and one that generates a random one
        public void Set(string args1, string args2, string pwd, string args3)
        {
            if (File.Exists(args2))
            {
                try
                {
                    byte[] vaultKey = CreateVaultKey(pwd, GetSecretKey(args1));

                    JsonVault jsonVault = JsonSerializer.Deserialize<JsonVault>(File.ReadAllText(args2)) ?? throw new ArgumentNullException("\nError: argument was null, command aborted");
                    byte[] encryptedVault = jsonVault.Vault;
                    byte[] IV = jsonVault.IV;

                    string pwdToAdd = PasswordUtilities.UserPasswordInput();
                    Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                    decryptedVault.Add(args3, pwdToAdd);

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
        public void Set(string args1, string args2, string pwd, string args3, string args4)
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

                        string pwdToAdd = PasswordUtilities.GenerateRandomPassword();
                        Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                        decryptedVault.Add(args3, pwdToAdd);

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

        // Delete-command method
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

                    Dictionary<string, string> decryptedVault = DecryptVault(encryptedVault, vaultKey, IV);
                    if (decryptedVault.ContainsKey(args3))
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

        // Secret-command method
        public void Secret(string filePath)
        {
            byte[] secretKey = GetSecretKey(filePath);
            string displayKey = Convert.ToBase64String(secretKey);
            Console.WriteLine(displayKey);
        }
        
    }
}
