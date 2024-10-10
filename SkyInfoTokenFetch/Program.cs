using System.Net.Http.Json;
using Newtonsoft.Json;
using TextCopy;

namespace SkyInfoTokenFetch;

internal static class Program
{
    [STAThread]
    public static async Task Main()
    {
        var appsettingsCaminho = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
        string email;
        string senha;
        string endpoint;

        await using (var appsettingsTexto = File.Open(appsettingsCaminho, FileMode.Open, FileAccess.ReadWrite))
        {
            using var streamReader = new StreamReader(appsettingsTexto);
            var configuração = JsonConvert.DeserializeObject<AppsettingsDto>(await streamReader.ReadToEndAsync());
            email = configuração.Email;
            senha = configuração.Senha;
            endpoint = configuração.Endpoint;
            var init = configuração.Init;

            if (!init)
            {
                configuração.Senha = ExtensõesDeCriptografiaAes.Encrypt(configuração.Senha);
                configuração.Init = true;

                appsettingsTexto.SetLength(0);
                appsettingsTexto.Seek(0, SeekOrigin.Begin);

                var conteúdoNovo = JsonConvert.SerializeObject(configuração);
                await using var streamWriter = new StreamWriter(appsettingsTexto);
                await streamWriter.WriteAsync(conteúdoNovo);
            }
            else
            {
                senha = ExtensõesDeCriptografiaAes.Decrypt(senha);
            }
        }

        var url = endpoint ?? "https://api.skyinfo.co/Autenticar";
        var corpoDaRequisição = new CorpoDaRequisição
        {
            Email = new Contato
            {
                TipoContato = "Email",
                Identificacao = email
            },
            Senha = senha
        };

        using var client = new HttpClient();
        try
        {
            var response = await client.PostAsJsonAsync(url, corpoDaRequisição);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<IEnumerable<RetornoAutenticação>>();
                await ClipboardService.SetTextAsync(result.Last().Token.AccessToken);
            }
            else
            {
                Console.WriteLine("Request failed. Status code: " + response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}

internal class RetornoAutenticação
{
    public Token Token { get; set; }
    public Token RefreshToken { get; set; }
    public Organização Organizacao { get; set; }
}

internal class Organização
{
    public string Id { get; set; }
    public string Nome { get; set; }
    public bool Desativado { get; set; }
}  

internal class Token
{
    public string AccessToken { get; set; }
    public DateTime DataExpiracao { get; set; }
}

internal class CorpoDaRequisição
{
    public string Senha { get; set; }
    public Contato Email { get; set; }
}

internal class Contato
{
    public string TipoContato { get; set; }
    public string Identificacao { get; set; }
}