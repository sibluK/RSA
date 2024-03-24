using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;

namespace RSA
{
    internal class Program
    {
        private static string publicKeyFile = "C:\\Users\\pugli\\OneDrive\\Stalinis kompiuteris\\Desktop\\Kolegija\\4 semestras\\INFORMACIJOS SAUGUMAS\\RSA\\publicKey.txt";
        private static string encryptedTextFile = "C:\\Users\\pugli\\OneDrive\\Stalinis kompiuteris\\Desktop\\Kolegija\\4 semestras\\INFORMACIJOS SAUGUMAS\\RSA\\encryptedText.txt";

        private static string ReadFromFile(string fileName)
        {
            using (StreamReader reader = new StreamReader(fileName))
            {
                return reader.ReadToEnd();
            }
        }

        private static void WriteToFile(string fileName, string text)
        {
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                writer.Write(text);
            }
        }

        private static bool IsPrime(BigInteger number)
        {
            if (number <= 1)
                return false;
            if (number <= 3)
                return true;

            if (number % 2 == 0 || number % 3 == 0)
                return false;

            for (int i = 5; i * i <= number; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0)
                    return false;
            }

            return true;
        }

        private static string createPublicKeyString(BigInteger n, BigInteger e)
        {
            return $"n = {n}\ne = {e}";
        }

        private static Tuple<BigInteger, BigInteger> findPrimes(BigInteger n)
        {
            BigInteger sqrtN = Sqrt(n);

            for (BigInteger i = 2; i <= sqrtN; i++)
            {
                if (n % i == 0 && IsPrime(i))
                {
                    BigInteger q = n / i;
                    if (IsPrime(q))
                    {
                        return Tuple.Create(i, q);
                    }
                }
            }

            return null;
        }

        private static BigInteger Sqrt(BigInteger n)
        {
            if (n == 0 || n == 1)
                return n;

            BigInteger start = 1, end = n, ans = 0;

            while (start <= end)
            {
                BigInteger mid = (start + end) / 2;

                if (mid * mid == n)
                    return mid;

                if (mid * mid < n)
                {
                    start = mid + 1;
                    ans = mid;
                }
                else
                {
                    end = mid - 1;
                }
            }

            return ans;
        }
        private static BigInteger euclidAlgorithm(BigInteger a, BigInteger b)
        {
            if (a == 0)
                return b;

            return euclidAlgorithm(b % a, a);
        }

        private static BigInteger extendedEuclidAlgorithm(BigInteger a, BigInteger b, ref BigInteger s, ref BigInteger t)
        {
            if (a == 0)
            {
                s = 0;
                t = 1;
                return b;
            }

            BigInteger s1 = 1, t1 = 0;
            BigInteger DBD = extendedEuclidAlgorithm(b % a, a, ref s1, ref t1);

            s = t1 - (b / a) * s1;
            t = s1;

            return DBD;
        }

        public static BigInteger ExtractNValue(string fileContent)
        {
            string[] lines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string line in lines)
            {
                string[] parts = line.Split('=');
                if (parts.Length == 2)
                {
                    string variable = parts[0].Trim();
                    string valueStr = parts[1].Trim();
                    if (variable.Equals("n", StringComparison.OrdinalIgnoreCase))
                    {
                        return BigInteger.Parse(valueStr);
                    }
                }
            }
            throw new Exception("Value of 'n' not found in the file.");
        }


        private static string encrypt(string tekstas, BigInteger n, BigInteger e)
        {
            StringBuilder encryptedText = new StringBuilder();

            foreach (char c in tekstas)
            {
                BigInteger asciiCode = new BigInteger(c);
                BigInteger encryptedChar = BigInteger.ModPow(asciiCode, e, n);

                encryptedText.Append(encryptedChar.ToString() + " ");
            }
            return encryptedText.ToString();
        }


        private static string decrypt()
        {
            BigInteger n = ExtractNValue(ReadFromFile(publicKeyFile));

            string encryptedText = ReadFromFile(encryptedTextFile);


            var primes = findPrimes(n);
            BigInteger p = primes.Item1;
            BigInteger q = primes.Item2;

            BigInteger phi = (p - 1) * (q - 1);

            BigInteger e = generateE(phi);

            BigInteger d = createPrivateKey(e, phi);

            StringBuilder decryptedText = new StringBuilder();

            string[] encryptedChars = encryptedText.Split(' ');
            foreach (string encryptedChar in encryptedChars)
            {
                if (!string.IsNullOrWhiteSpace(encryptedChar))
                {
                    try
                    {
                        BigInteger encryptedCode = BigInteger.Parse(encryptedChar);
                        BigInteger decryptedCode = BigInteger.ModPow(encryptedCode, d, n);

                        char decryptedChar = (char)int.Parse(decryptedCode.ToString());
                        decryptedText.Append(decryptedChar);
                    }
                    catch (FormatException ex)
                    {
                        Console.WriteLine($"Failed to parse value: {encryptedChar}. Error: {ex.Message}");
                    }
                }
            }

            return decryptedText.ToString();

        }

        private static Tuple<BigInteger, BigInteger> createPublicKey(BigInteger p, BigInteger q)
        {
            BigInteger n = p * q;

            BigInteger phi = (p - 1) * (q - 1);

            BigInteger e = generateE(phi);

            return Tuple.Create(n, e);
        }

        private static BigInteger createPrivateKey(BigInteger e, BigInteger phi)
        {
            BigInteger s = 0, t = 0;
            BigInteger gcd = extendedEuclidAlgorithm(e, phi, ref s, ref t);

            BigInteger d = (s % phi + phi) % phi;

            return d;
        }

        private static BigInteger generateE(BigInteger phi)
        {
            BigInteger e = 3;

            while (euclidAlgorithm(e, phi) != 1)
            {
                e += 2;
            }

            return e;
        }


        public static void Main(string[] args)
        {
            ///*
            bool run = true;
            while (run)
            {
                Console.WriteLine("----------------------------------------------------------");
                Console.WriteLine("Pasirinkite:");
                Console.WriteLine("1. Sifruoti (RSA)");
                Console.WriteLine("2. Desifruoti (RSA)");
                Console.WriteLine("3. Baigti.");
                Console.WriteLine();
                Console.Write("Pasirinkimas: ");
                string pasirinkimas = Console.ReadLine();

                switch (pasirinkimas)
                {
                    case "1":
                        Console.Write("Iveskite teksta: ");
                        string tekstas = Console.ReadLine();
                        Console.Write("Iveskite p: ");
                        BigInteger p = BigInteger.Parse(Console.ReadLine());

                        while (!IsPrime(p))
                        {
                            Console.Write("p nera pirminis skaicius, iveskite kita: ");
                            p = BigInteger.Parse(Console.ReadLine());
                        }

                        Console.Write("Iveskite q: ");
                        BigInteger q = BigInteger.Parse(Console.ReadLine());
                        while (!IsPrime(q))
                        {
                            Console.Write("q nera pirminis skaicius, iveskite kita: ");
                            q = BigInteger.Parse(Console.ReadLine());
                        }

                        var publicKey = createPublicKey(p, q);
                        BigInteger n = publicKey.Item1;
                        BigInteger e = publicKey.Item2;

                        string publicKeyString = createPublicKeyString(n, e);
                        WriteToFile(publicKeyFile, publicKeyString);

                        string encryptedText = encrypt(tekstas, n, e);
                        WriteToFile(encryptedTextFile, encryptedText);

                        break;

                    case "2":
                        Console.WriteLine("----------------------------------------------------------");
                        Console.WriteLine($"Desifruotas tekstas: {decrypt()}");
                        break;

                    case "3":
                        run = false;
                        break;

                    default:
                        Console.WriteLine();
                        Console.WriteLine("Tokio pasirinkimo nėra!");
                        break;
                }
            }
            //*/
        }
    }
}
