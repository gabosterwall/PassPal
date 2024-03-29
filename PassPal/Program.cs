﻿namespace PassPal
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
                // Help-command
                if (args[0].ToLower() == "help")
                {
                    Console.WriteLine(@"
                    Available commands:

                    init   -  initialize new client-file and corresponding server-file         
                    create -  create a new client-file for existing server-file, i.e new login
                    get    -  display specific password or all current properties in the vault
                    set    -  add or update an existing password in the vault
                    delete -  remove a password from the vault
                    secret -  display the secret key stored in a client");
                }

                // Init-command
                if (args[0].ToLower() == "init" && args.Length == 3)
                {
                    Console.WriteLine("\nEnter new master password: ");
                    fileManager.Init(args[1], args[2], PasswordUtilities.UserPasswordInput());
                }

                // Create-command
                if (args[0].ToLower() == "create" && args.Length == 3)
                {
                    Console.WriteLine("\nEnter your password: ");
                    fileManager.Create(args[1], args[2], PasswordUtilities.UserPasswordInput());
                }

                // Get-command
                if (args[0].ToLower() == "get" && args.Length == 3) // To list all stored props
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        fileManager.Get(args[1], args[2], PasswordUtilities.UserPasswordInput());
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }
                if (args[0].ToLower() == "get" && args.Length == 4) // To list specific password connected to inputed prop
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        fileManager.Get(args[1], args[2], PasswordUtilities.UserPasswordInput(), args[3]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }

                // Set-command
                if (args[0].ToLower() == "set" && args.Length == 4) // To set a new password in the vault
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        fileManager.Set(args[1], args[2], PasswordUtilities.UserPasswordInput(), args[3]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }
                if (args[0].ToLower() == "set" && args.Length == 5) // To set a randomly generated password according to Regex: [a-zA-Z0-9]{20}
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        fileManager.Set(args[1], args[2], PasswordUtilities.UserPasswordInput(), args[3], args[4]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }

                // Delete-command
                if (args[0].ToLower() == "delete")
                {
                    if (File.Exists(args[1]))
                    {
                        Console.WriteLine("\nEnter your password: ");
                        fileManager.Delete(args[1], args[2], args[3], PasswordUtilities.UserPasswordInput());
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
                }

                // Secret-command
                if (args[0].ToLower() == "secret" && args.Length == 2)
                {
                    if (File.Exists(args[1]))
                    {
                        fileManager.Secret(args[1]);
                    }
                    else
                        Console.WriteLine($"\nError:'{args[1]}' could not be found, command aborted.");
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