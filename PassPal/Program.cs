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
            if (args[0].Length != 0)
            {
                //Client client = new Client();
                //Server server = new Server();
                FileManager fileManager = new FileManager();

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
                    string masterPass = PasswordUtilities.NewPassword();
                    fileManager.Init(args[1], args[2], masterPass);
                }

                // [create] kommando: skapar ny client som läser in hemlig nyckel från den förstskapade filen om den existerar
                if (args[0].ToLower() == "create")
                {
                    string inputPass = PasswordUtilities.InputPassword();
                    byte[] secretKey = PasswordUtilities.InputSecretKey();
                    fileManager.Create(args[1], args[2], inputPass, secretKey);
                }

                // [get] kommando: frågar om lösenord, om man anger specifik prop skrivs dess lösenord, om inte listas samtliga prop
                if (args[0].ToLower() == "get" && args.Length < 4)
                {
                    string inputPass = PasswordUtilities.InputPassword();
                    fileManager.Get(args[1], args[2], inputPass);
                }
                if (args[0].ToLower() == "get" && args.Length > 3)
                {
                    string inputPass = PasswordUtilities.InputPassword();
                    fileManager.Get(args[1], args[2], args[3], inputPass);
                }

                // [set] kommando: 
                if (args[0].ToLower() == "set" && args.Length < 5)
                {
                    string inputPass = PasswordUtilities.InputPassword();
                    fileManager.Set(args[1], args[2], args[3], inputPass, false);
                }
                if (args[0].ToLower() == "set" && args.Length > 4)
                {
                    if (args[4].ToLower() == "-g" || args[4].ToLower() == "--generate")
                    {
                        string inputPass = PasswordUtilities.InputPassword();
                        fileManager.Set(args[1], args[2], args[3], inputPass, true);
                    }
                    else
                        Console.WriteLine($"\n Error: supplied flag not found, command aborted.");
                }

                // [delete] kommando: 
                if (args[0].ToLower() == "delete")
                {
                    string inputPass = PasswordUtilities.InputPassword();
                    fileManager.Delete(args[1], args[2], args[3], inputPass);
                }

                // [secret] kommando: skriver ut lagrad hemlig nyckel i specificerad client om en sådan finns
                if (args[0].ToLower() == "secret")
                {
                    fileManager.Secret(args[1]);
                }

                // [exit] kommando: avslutar sessionen
                if (args[0].ToLower() == "exit")
                {
                    Console.WriteLine("Thank you for using PassPal!");
                    Environment.Exit(0);
                }
            }
        }
    }
}