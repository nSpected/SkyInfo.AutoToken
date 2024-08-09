using System.Security.Cryptography;

namespace SkyInfoTokenFetch;

public static class ExtensõesDeCriptografiaAes
{

    private static readonly byte[] Chave = "1ac7f962e8ba4931"u8.ToArray();
    private static readonly byte[] Iv = "bc3ab294532de6da"u8.ToArray();

    public static string Encrypt(string plainText)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = Chave;
        aesAlg.IV = Iv;

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
        using MemoryStream msEncrypt = new MemoryStream();
        using CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write);
        using (var swEncrypt = new StreamWriter(csEncrypt))
        {
            swEncrypt.Write(plainText);
        }
        return Convert.ToBase64String(msEncrypt.ToArray());
    }

    public static string Decrypt(string encryptedText)
    {
        var cipherText = Convert.FromBase64String(encryptedText);

        using Aes aesAlg = Aes.Create();
        aesAlg.Key = Chave;
        aesAlg.IV = Iv;
        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using MemoryStream msDecrypt = new MemoryStream(cipherText);
        using CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using StreamReader srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    private static Aes ObterAes()
    {
        var aes = Aes.Create();
        aes.Key = Chave;
        aes.IV = Iv;

        return aes;
    }
}