namespace PassPal
{
    public class Program
    {
        public static void Main(string[] args)
        {
            /*
             *  PassPal command-line application 
             *  
             *  By Gabriél Österwall, Henrik Widercrantz, Josefine Bäckström
             *  2023-03-09
             * 
             */

            FileManager fileManager = new FileManager();

            if(args.Length != 0)
            {
                // [help] kommando: skriver ut programmets alla kommandon med en kort beskrivning över vad dem gör
                if (args[0].ToLower() == "help")
                {
                    Console.WriteLine(@"
                    Available commands:

                    init   -  initialize new client-file and corresponding server-file:         <init> <client.json> <server.json>
                    create -  create a new client-file for existing server-file, i.e new login: <create> <client.mobile.json> <server.json>
                    get    -  display specific password or all current properties in the vault: <get> <client.json> <server.json> [prop]
                    set    -  add or update an existing password in the vault:                  <set> <client.json> <server.json> <prop> [-g / --generate]
                    delete -  remove a password from the vault:                                 <delete> <client.json> <server.json> <prop>
                    secret -  display the secret key stored in a client:                        <secret> <client.json>
                    exit   -  exit the program.");
                }

                // [init] kommando: skapar client, frågar om lösenord, skapar server
                if (args[0].ToLower() == "init")
                {
                    Console.WriteLine("\nEnter new master password: ");
                    string masterPass = PasswordUtilities.UserInput();
                    fileManager.Init(args[1], args[2], masterPass);
                }

                // [create] kommando: skapar ny client som läser in hemlig nyckel från den förstskapade filen om den existerar
                if (args[0].ToLower() == "create")
                {
                    Console.WriteLine("\nEnter your password: ");
                    string inputPass = PasswordUtilities.UserInput();
                    Console.WriteLine("\nEnter your secret key: ");
                    string secretKey = PasswordUtilities.UserInput();
                    fileManager.Create(args[1], args[2], inputPass, secretKey);
                }

                // [Get]-kommando: 
                if (args[0].ToLower() == "get" && args.Length == 3)
                {
                    Console.WriteLine("\nEnter your password: ");
                    string inputPass = PasswordUtilities.UserInput();
                    fileManager.Get(args[1], args[2], inputPass);
                }
                if (args[0].ToLower() == "get" && args.Length == 4)
                {
                    Console.WriteLine("\nEnter your password: ");
                    string inputPass = PasswordUtilities.UserInput();
                    fileManager.Get(args[1], args[2], inputPass, args[3]);
                }

                // [Set]-kommando:
                if (args[0].ToLower() == "set" && args.Length == 4)
                {
                    Console.WriteLine("\nEnter your password: ");
                    string inputPass = PasswordUtilities.UserInput();
                    Console.WriteLine($"\nEnter the password you want to store for '{args[3]}': ");
                    string pwdToAdd = PasswordUtilities.UserInput();
                    fileManager.Set(args[1], args[2], args[3], inputPass, pwdToAdd);
                }
                if (args[0].ToLower() == "set" && args.Length == 5)
                {
                    Console.WriteLine("\nEnter your password: ");
                    string inputPass = PasswordUtilities.UserInput();
                    string pwdToAdd = PasswordUtilities.GenerateRandomPassword();
                    fileManager.Set(args[1], args[2], args[3], args[4], inputPass, pwdToAdd);
                }

                // [delete] kommando: 
                if (args[0].ToLower() == "delete")
                {
                    Console.WriteLine("\nEnter your password: ");
                    string inputPass = PasswordUtilities.UserInput();
                    fileManager.Delete(args[1], args[2], args[3], inputPass);
                }

                // [secret] kommando: skriver ut lagrad hemlig nyckel i specificerad client om en sådan finns
                if (args[0].ToLower() == "secret" && args.Length == 2)
                {
                    if (File.Exists(args[1]))
                    {
                        string filePath = args[1];
                        fileManager.Secret(args[1]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }

                // [exit] kommando: avslutar sessionen
                if (args[0].ToLower() == "exit")
                {
                    Console.WriteLine("\nThank you for using PassPal!");
                    Environment.Exit(0);
                }
            }
            
        }
    }
}