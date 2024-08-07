using Newtonsoft.Json;

namespace SkyInfoTokenFetch;

public class AppsettingsDto
{
    [JsonConstructor]
    public AppsettingsDto(string email, string senha, string endpoint, bool init = false)
    {
        Email = email;
        Senha = senha;
        Endpoint = endpoint;
        Init = init;
    }

    public string Email { get; set; }
    public string Senha { get; set; }
    public string Endpoint { get; set; }
    public bool Init { get; set; } = false;
}