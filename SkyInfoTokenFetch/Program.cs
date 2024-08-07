using System.Net.Http.Json;
using Newtonsoft.Json;
using TextCopy;

namespace SkyInfoTokenFetch;

internal static class Program
{
    [STAThread]
    static async Task Main()
    {
        var appsettingsCaminho = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");

        var email = "";
        var senha = "";
        var endpoint = "";
        var init = false;

        await using (var appsettingsTexto = File.Open(appsettingsCaminho, FileMode.Open, FileAccess.ReadWrite))
        {
            using var streamReader = new StreamReader(appsettingsTexto);
            var configuração = JsonConvert.DeserializeObject<AppsettingsDto>(await streamReader.ReadToEndAsync());
            email = configuração.Email;
            senha = configuração.Senha;
            endpoint = configuração.Endpoint;
            init = configuração.Init;

            if (!init)
            {
                configuração.Senha = ExtensõesDeCriptografiaAes.CriptografarTexto(configuração.Senha);
                configuração.Init = true;

                appsettingsTexto.SetLength(0);
                appsettingsTexto.Seek(0, SeekOrigin.Begin);

                var conteúdoNovo = JsonConvert.SerializeObject(configuração);
                using var streamWriter = new StreamWriter(appsettingsTexto);
                await streamWriter.WriteAsync(conteúdoNovo);
            }
            else
            {
                senha = ExtensõesDeCriptografiaAes.DescriptografarTexto(senha);
            }
        }

        string url = endpoint ?? "https://api.skyinfo.co/Autenticar";
        CorpoDaRequisição corpoDaRequisição = new CorpoDaRequisição()
        {
            Email = new Contato()
            {
                TipoContato = "Email",
                Identificacao = email
            },
            Senha = senha
        };

        using var client = new HttpClient();
        try
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(url, corpoDaRequisição);
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