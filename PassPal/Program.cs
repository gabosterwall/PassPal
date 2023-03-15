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
             *  Created: 2023-03-15
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

                    init   -  initialize new client-file and corresponding server-file         
                    create -  create a new client-file for existing server-file, i.e new login
                    get    -  display specific password or all current properties in the vault
                    set    -  add or update an existing password in the vault
                    delete -  remove a password from the vault
                    secret -  display the secret key stored in a client
                    exit   -  exit the program");
                }

                // Init-kommando
                if (args[0].ToLower() == "init" && args.Length == 3)
                {
                    Console.WriteLine("\nEnter new master password: ");
                    //string masterPass = PasswordUtilities.UserInput();
                    fileManager.Init(args[1], args[2], PasswordUtilities.UserPasswordInput());
                }

                // Create-kommando
                if (args[0].ToLower() == "create" && args.Length == 3)
                {
                    Console.WriteLine("\nEnter your password: ");
                    string inputPass = PasswordUtilities.UserPasswordInput();
                    Console.WriteLine("\nEnter your secret key: ");
                    string secretKey = PasswordUtilities.UserKeyInput();
                    fileManager.Create(args[1], args[2], inputPass, secretKey);
                }

                // Get-kommando
                if (args[0].ToLower() == "get" && args.Length == 3) // För att lista alla props
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        string inputPass = PasswordUtilities.UserPasswordInput();
                        fileManager.Get(args[1], args[2], inputPass);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }
                if (args[0].ToLower() == "get" && args.Length == 4) // För att lista lösenord kopplat till specifikt prop
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        string inputPass = PasswordUtilities.UserPasswordInput();
                        fileManager.Get(args[1], args[2], inputPass, args[3]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }

                // Set-kommando
                if (args[0].ToLower() == "set" && args.Length == 4) // För att ange eget lösenord
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        string inputPass = PasswordUtilities.UserPasswordInput();
                        fileManager.Set(args[1], args[2], inputPass, args[3]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }
                if (args[0].ToLower() == "set" && args.Length == 5) // För att generera ett lösenord enligt Regex: [a-zA-Z0-9]{20}
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        string inputPass = PasswordUtilities.UserPasswordInput();
                        fileManager.Set(args[1], args[2], inputPass, args[3], args[4]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }

                // Delete-kommando
                if (args[0].ToLower() == "delete")
                {
                    Console.WriteLine("\nEnter your password: ");
                    string inputPass = PasswordUtilities.UserPasswordInput();
                    fileManager.Delete(args[1], args[2], args[3], inputPass);
                }

                // Secret-kommando
                if (args[0].ToLower() == "secret" && args.Length == 2)
                {
                    if (File.Exists(args[1]))
                    {
                        fileManager.Secret(args[1]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }

                // Exit-kommando
                if (args[0].ToLower() == "exit")
                {
                    Console.WriteLine("\nThank you for using PassPal!");
                    Environment.Exit(0);
                }
            }
            else
            {
                Console.WriteLine("\nHello and welcome to PassPal, your handy password manager command-line application!");
                Console.WriteLine("\nFor a list of available commands, please type 'help'");
            }
                
        }
    }
}