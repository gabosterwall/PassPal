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
    public static class PasswordUtilities
    {
       
        // OBS! För samtliga metoder har kod hämtats från (källa)
        
        public static string GenerateRandomPassword() 
        {
            const int length = 20;
            const string alphaNumericalChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] passwordChars = new char[length];

            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                byte[] rngBytes = new byte[length];
                rng.GetBytes(rngBytes);

                for (int i = 0; i < length; i++)
                {
                    int rngIndex = rngBytes[i] % alphaNumericalChars.Length;
                    passwordChars[i] = alphaNumericalChars[rngIndex];
                }
            }
            string generatedInput = new string(passwordChars);
            Regex regex = new Regex(@"^[a-zA-Z0-9]");
            string generatedOutput = regex.Replace(generatedInput, "");

            if (generatedInput.Length == 20)
                return generatedOutput;
            else
                throw new Exception("\nError: password not generate properly.");
        }

        public static string UserInput()
        {
            string userInput = string.Empty;

            while(string.IsNullOrEmpty(userInput))
            {
                userInput = Console.ReadLine()!;

                if (userInput == null || userInput == "")
                    Console.WriteLine("\nError: null or empty input value.");
            }
            return userInput;

            //Console.WriteLine($"\n Enter your secret key, then press [Enter]");
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
