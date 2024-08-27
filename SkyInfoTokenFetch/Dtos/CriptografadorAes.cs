using System.Security.Cryptography;
using System.Text;

namespace SkyInfoTokenFetch.Dtos;

public class CriptografadorAes
{
    private byte[] chave { get; init; }
    private byte[] vetorDeInicialização { get; init; }
    public CriptografadorAes(string chave = "")
    {
        if (string.IsNullOrEmpty(chave) || chave.Length < 16)
        {
            this.chave = Guid.NewGuid().ToByteArray();
            vetorDeInicialização = Guid.NewGuid().ToByteArray();
            return;
        }

        var chaveLimitadaA16Caracteres = chave.Chunk(16).ElementAt(0);
        this.chave = Encoding.UTF8.GetBytes(chaveLimitadaA16Caracteres);
        vetorDeInicialização = Encoding.UTF8.GetBytes(chaveLimitadaA16Caracteres
            .Reverse().ToArray());
    }

    public string Criptografar(string conteúdo)
    {
        using var aes = ObterAes();
        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using var streamWriter = new StreamWriter(cryptoStream);
        streamWriter.Write(conteúdo);

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public string Descriptografar(string conteúdoCriptografado)
    {
        var bytesCriptografadas = Convert.FromBase64String(conteúdoCriptografado);

        using var aes = ObterAes();
        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

        using var memoryStream = new MemoryStream(bytesCriptografadas);
        using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamWriter = new StreamReader(cryptoStream);
        return streamWriter.ReadToEnd();
    }

    private Aes ObterAes()
    {
        Aes aes = Aes.Create();
        aes.Key = chave;
        aes.IV = vetorDeInicialização;

        return aes;
    }
}