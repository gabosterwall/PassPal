using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PassPal
{
    internal static class PasswordUtilities
    {
        // OBS! För samtliga metoder har kod hämtats från (källa)
        public static string NewPassword()
        {
            Console.WriteLine($"\n Input new master password, then press [Enter]");
            string masterPass = string.Empty;
            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && masterPass.Length > 0)
                {
                    Console.Write("\b \b");
                    masterPass = masterPass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    masterPass += keyInfo.KeyChar;
                }
            }
            while (key != ConsoleKey.Enter);
            return masterPass;

        }
        public static string InputPassword()
        {
            Console.WriteLine($"\n Enter your password, then press [Enter]");
            string pass = string.Empty;

            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && pass.Length > 0)
                {
                    Console.Write("\b \b");
                    pass = pass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    pass += keyInfo.KeyChar;
                }
            }
            while (key != ConsoleKey.Enter);
            return pass;
        }
        public static string AddPassword()
        {
            Console.WriteLine($"\n Enter the password you want to store, then press [Enter]");
            string newPass = string.Empty;

            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && newPass.Length > 0)
                {
                    Console.Write("\b \b");
                    newPass = newPass[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    newPass += keyInfo.KeyChar;
                }
            }
            while (key != ConsoleKey.Enter);
            return newPass;
        }
        public static string GenerateRandomPassword() //Mycket inspiration från Stack, fråga om OK annars komma på ny helt originell kod...
        {
            int passLength = 20;
            string alphaNumericChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            byte[] passBytes = new byte[passLength];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(passBytes);
            }

            char[] charArr = new char[passLength];
            for (int i = 0; i < passLength; i++)
                charArr[i] = alphaNumericChars[passBytes[i] % alphaNumericChars.Length];

            return new string(charArr);
        }

        public static byte[] InputSecretKey()
        {
            Console.WriteLine($"\n Enter your secret key, then press [Enter]");
            string inputKey = string.Empty;

            ConsoleKey key;
            do
            {
                var keyInfo = Console.ReadKey(intercept: true);
                key = keyInfo.Key;

                if (key == ConsoleKey.Backspace && inputKey.Length > 0)
                {
                    Console.Write("\b \b");
                    inputKey = inputKey[0..^1];
                }
                else if (!char.IsControl(keyInfo.KeyChar))
                {
                    Console.Write("*");
                    inputKey += keyInfo.KeyChar;
                }
            }
            while (key != ConsoleKey.Enter);
            byte[] inputToBytes = Convert.FromBase64String(inputKey);
            return inputToBytes;
        }
    }
}
