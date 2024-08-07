using System.Security.Cryptography;
using System.Text;

namespace SkyInfoTokenFetch;

public static class ExtensõesDeCriptografiaAes
{
    private static readonly byte[] Chave = Encoding.ASCII.GetBytes("663abe6618694825");
    private static readonly byte[] Iv = Encoding.ASCII.GetBytes("8675842356F04F8B");

    public static string CriptografarTexto(string texto)
    {
        using var aes = ObterAes();
        using var criptografador = aes.CreateEncryptor(aes.Key, aes.IV);

        using var msEncrypt = new MemoryStream();
        using var csEncrypt = new CryptoStream(msEncrypt, criptografador, CryptoStreamMode.Write);
        using var swEncrypt = new StreamWriter(csEncrypt);
        swEncrypt.Write(texto);
        csEncrypt.FlushFinalBlock();

        var resultado = msEncrypt.ToArray();
        return Convert.ToBase64String(resultado);
    }

    public static string DescriptografarTexto(string textoCriptografado)
    {
        using var aes = ObterAes();
        using var descriptografador = aes.CreateDecryptor(aes.Key, aes.IV);
        var bytes = Convert.FromBase64String(textoCriptografado);
        using var msDecrypt = new MemoryStream(bytes);
        using var csDecrypt = new CryptoStream(msDecrypt, descriptografador, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);
        return srDecrypt.ReadToEnd();
    }

    private static Aes ObterAes()
    {
        var aes = Aes.Create();
        aes.Key = Chave;
        aes.IV = Iv;
        aes.Padding = PaddingMode.PKCS7;

        return aes;
    }
}