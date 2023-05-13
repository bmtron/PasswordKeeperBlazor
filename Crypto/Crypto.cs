using System.Security.Cryptography;
using System.Text;

namespace Crypto;

public class Crypto
{
    private const int KeySize = 128;

    private const int DerivationIterations = 65536;

    public static string Encrypt(string plainText, string passPhrase)
    {
        var saltStringBytes = Generate128BitsOfRandomEntropy();
        var ivStringBytes = Generate128BitsOfRandomEntropy();
        var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(KeySize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 128;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                symmetricKey.KeySize = KeySize;
                using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                        {
                            cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                            cryptoStream.FlushFinalBlock();
                            var cipherTextBytes = saltStringBytes;
                            cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                            var test = memoryStream.ToArray();
                            cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                            memoryStream.Close();
                            cryptoStream.Close();
                            var t = Convert.ToBase64String(memoryStream.ToArray());
                            return Convert.ToBase64String(cipherTextBytes);
                        }
                    }
                }
            }
        }
    }

    public static string Decrypt(string cipherText, string passPhrase)
    {
        var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);

        var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(KeySize / 8).ToArray();

        var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(KeySize / 8).Take(KeySize / 8).ToArray();

        var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((KeySize / 8) * 2)
            .Take(cipherTextBytesWithSaltAndIv.Length - ((KeySize / 8) * 2)).ToArray();

        using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
        {
            var keyBytes = password.GetBytes(KeySize / 8);
            using (var symmetricKey = new RijndaelManaged())
            {
                symmetricKey.BlockSize = 128;
                symmetricKey.Mode = CipherMode.CBC;
                symmetricKey.Padding = PaddingMode.PKCS7;
                using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                {
                    using (var memoryStream = new MemoryStream(cipherTextBytes))
                    {
                        using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                        {
                            var plainTextBytes = new byte[cipherTextBytes.Length];
                            var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                            memoryStream.Close();
                            cryptoStream.Close();

                            return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                        }
                    }
                }
            }
        }
    }

    private static byte[] Generate128BitsOfRandomEntropy()
    {
        var randomBytes = new byte[16];
        using (var rngCsp = new RNGCryptoServiceProvider())
        {
            rngCsp.GetBytes(randomBytes);
        }

        return randomBytes;
    }

    private static string hash(string plainPass)
    {
        int iterations = DerivationIterations;
        var saltBytes = Generate128BitsOfRandomEntropy();
        var password = new Rfc2898DeriveBytes(plainPass, saltBytes, iterations);
        var keyBytes = password.GetBytes(512 / 8);
        var t = Convert.ToBase64String(saltBytes).Replace("=", "") + Convert.ToBase64String(keyBytes).Replace("=", "");

        return t;
    }

    private static string hash(string plainPass, string saltString)
    {
        int iterations = DerivationIterations;
        if (saltString.Length % 3 != 0)
        {
            while (saltString.Length % 3 != 0)
            {
                saltString = saltString + "=";
            }
        }

        var saltBytes = Convert.FromBase64String(saltString);
        var password = new Rfc2898DeriveBytes(plainPass, saltBytes, iterations);
        var keyBytes = password.GetBytes(512 / 8);
        var result = Convert.ToBase64String(saltBytes).Replace("=", "") +
                     Convert.ToBase64String(keyBytes).Replace("=", "");

        return result;
    }

    public static bool compareHash(string plainPass, string hashPass)
    {
        string splitHash = hashPass.Substring(0, 22);

        return hash(plainPass, splitHash).Equals(hashPass);
    }
}