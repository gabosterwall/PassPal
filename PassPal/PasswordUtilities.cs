using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PassPal
{
    public static class PasswordUtilities   // User input methods + password generator in case of -g or --generate
    {

        public static string GenerateRandomPassword()
        {
            const int pwdSize = 20;
            const string alphaNumericalChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            byte[] rngBytes = new byte[pwdSize];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(rngBytes);
            }

            // Now we have 20 random bytes in rngBytes


            // Build the string-password with alphaNumericalChars:
            string generatedPwd = string.Empty;
            foreach (byte item in rngBytes)
            {
                generatedPwd += alphaNumericalChars[item % alphaNumericalChars.Length];
            }

            // Match with required Regex: 

            Regex regex = new Regex(@"^[a-zA-Z0-9]{20}$");

            if (!regex.IsMatch(generatedPwd))
                throw new Exception("\nError: password not generated properly.");
            else
                return generatedPwd;
        }

        public static string UserKeyInput()
        {
            string userInput = string.Empty;

            while (string.IsNullOrEmpty(userInput))
            {
                userInput = Console.ReadLine() ?? throw new ArgumentNullException("\nError: null value input.");

                if (userInput == null || userInput == "")
                    Console.WriteLine("\nError: null or empty input value.");
                else
                    break;
            }
            return userInput;
        }

        // Method for user input of master password; commented code are to be implemented after submitted assignment
        public static string UserPasswordInput()
        {
            string userInput = string.Empty;

            while(string.IsNullOrEmpty(userInput) /*|| userInput.Length < 20*/)                                                                         
            {
                userInput = Console.ReadLine() ?? throw new ArgumentNullException("\nNull value or empty input.");

                if (userInput == null || userInput == "")
                    Console.WriteLine("\nError: null or empty input value.");
                //else if (userInput.Length < 20)                                                                                                      
                //    Console.WriteLine("\nError: password must be at least 20 characters long.");
            }
            return userInput;


            // The following code has been inspired from a user on Stack Overflow: https://stackoverflow.com/questions/3404421/password-masking-console-application
            // It is not a part of this assignment, only for future implementations

            //Console.WriteLine($"\n Enter your secret key, then press [Enter]"); ===> Från Stack, men funkar ändå inte för integrationstesterna        
            //string inputKey = string.Empty;
            //ConsoleKey key;
            //do
            //{
            //    var keyInfo = Console.ReadKey(intercept: true);
            //    key = keyInfo.Key;

            //    if (key == ConsoleKey.Backspace && inputKey.Length > 0)
            //    {
            //        Console.Write("\b \b");
            //        inputKey = inputKey[0..^1];
            //    }
            //    else if (!char.IsControl(keyInfo.KeyChar))
            //    {
            //        Console.Write("*");
            //        inputKey += keyInfo.KeyChar;
            //    }
            //}
            //while (key != ConsoleKey.Enter);
            //byte[] inputToBytes = Convert.FromBase64String(inputKey);
            //return inputToBytes;
        }
    }
}
