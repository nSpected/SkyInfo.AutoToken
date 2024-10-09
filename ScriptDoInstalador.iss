#define NomeDaAplicacao "SkyInfo.AutoToken"
#define NomeDaEmpresa "Sky Informática Ltda."
#define UrlDaAplicacao "https://www.skyinfo.co/"
#define NomeDoExecutavelDaAplicacao "SkyInfoTokenFetch.exe"
; #define CaminhoDaFonteDaAplicacao "SkyInfoTokenFetch"
#define CaminhoDeSaidaDoInstaladorCompilado ".\Compilado"

[Setup]
AppId={{007c0d89-0db4-48be-804d-65e12bbf1bff}}
AppName={#NomeDaAplicacao}
AppVersion={#Versao}
AppPublisher={#NomeDaEmpresa}
AppPublisherURL={#UrlDaAplicacao}
AppSupportURL={#UrlDaAplicacao}
AppUpdatesURL={#UrlDaAplicacao}
DefaultDirName={autopf}\{#NomeDaAplicacao}
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
DefaultGroupName={#NomeDaAplicacao}
PrivilegesRequired=lowest
OutputDir=D:\a\SkyInfo.AutoToken\SkyInfo.AutoToken\Instalador
OutputBaseFilename={#NomeDaAplicacao}.Instalador
Compression=lzma
SolidCompression=yes
WizardStyle=modern
UsePreviousAppDir=yes

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"

[Files]
Source: "{#CaminhoDaFonteDaAplicacao}\{#NomeDoExecutavelDaAplicacao}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#CaminhoDaFonteDaAplicacao}\*"; DestDir: "{app}"; Flags: recursesubdirs createallsubdirs ignoreversion

[Icons]
Name: "{userdesktop}\{#NomeDaAplicacao}"; Filename: "{app}\{#NomeDoExecutavelDaAplicacao}"; Tasks: desktopicon

[Tasks]
Name: desktopicon; Description: "Criar ícone na área de trabalho.";

[Code]
var
  PaginaDeCredenciaisDoUsuario: TInputQueryWizardPage;

procedure InitializeWizard();
begin
  // Criação da página de credenciais
  PaginaDeCredenciaisDoUsuario := CreateInputQueryPage(
    wpSelectDir,
    'Configuração',
    'Por favor, preencha as os seguintes campos:',
    'Não se esquece de desinstalar a versão anterior antes de instalar a nova (eu não sei fazer isso automático ainda).'
  );

  // Adiciona as páginas no instalador
  PaginaDeCredenciaisDoUsuario.Add('Url de Autenticação:', False);
  PaginaDeCredenciaisDoUsuario.Add('Email:', False);
  PaginaDeCredenciaisDoUsuario.Add('Senha:', True);
  PaginaDeCredenciaisDoUsuario.Values[0] := ExpandConstant('https://api.skyinfo.co/Autenticar');
end;

// Função para enviar requisição POST com credenciais
function EnviarRequisicaoDeValidacao(Email, Senha, Url: String): Boolean;
var
  HttpRequest: Variant;
  ResponseStatus: Integer;
  Data: String;
begin
  Result := False;
  Data := Format('{"email":{"tipoContato":"email","identificacao":"%s"},"senha":"%s"}', [Email, Senha]);

  try
    HttpRequest := CreateOleObject('WinHttp.WinHttpRequest.5.1');
    HttpRequest.Open('POST', Url, False);
    HttpRequest.SetRequestHeader('Content-Type', 'application/json');
    HttpRequest.Send(Data);
    ResponseStatus := HttpRequest.Status;

    if ResponseStatus = 200 then
    begin
      Result := True;
    end;
  except
    MsgBox('Eh, não deu certo. Verifique sua conexão com a internet.', mbError, MB_OK);
  end;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
var
  Email, Senha, Endpoint: String;
begin
  Result := True;
  if CurPageID = PaginaDeCredenciaisDoUsuario.ID then
  begin
    Endpoint := PaginaDeCredenciaisDoUsuario.Values[0];
    Email := PaginaDeCredenciaisDoUsuario.Values[1];
    Senha := PaginaDeCredenciaisDoUsuario.Values[2];

    if not EnviarRequisicaoDeValidacao(Email, Senha, Endpoint) then
    begin
      MsgBox('Credenciais inválidas. Tenta mais uma vez aí e vê se vai. (ou o endpoint tá errado 🤔)', mbError, MB_OK);
      Result := False;
    end;
  end;
end;

// Função para dividir strings
function DividirString(const S: String; const Delimiter: String): TArrayOfString;
var
  Count, Pos, Start, DelimiterLength: Integer;
begin
  Count := 0;
  Pos := 1;
  DelimiterLength := Length(Delimiter);
  while Pos <= Length(S) do
  begin
    Start := Pos;
    while (Pos <= Length(S)) and (Copy(S, Pos, DelimiterLength) <> Delimiter) do
      Inc(Pos);
    SetArrayLength(Result, Count + 1);
    Result[Count] := Copy(S, Start, Pos - Start);
    Inc(Count);
    Pos := Pos + DelimiterLength;
  end;
end;

// Função para procurar e substituir uma string
function SubstituirString(const S, OldPattern, NewPattern: String): String;
var
  ResultString: String;
  SearchPos: Integer;
begin
  ResultString := S;
  SearchPos := Pos(OldPattern, ResultString);
  while SearchPos > 0 do
  begin
    Delete(ResultString, SearchPos, Length(OldPattern));
    Insert(NewPattern, ResultString, SearchPos);
    SearchPos := Pos(OldPattern, ResultString);
  end;
  Result := ResultString;
end;

function ObterStringDoArquivo(const NomeDoArquivo: String): String;
var
  Linhas: TArrayOfString;
  I: Integer;
begin
  Result := '';
  if LoadStringsFromFile(NomeDoArquivo, Linhas) then
  begin
    for I := 0 to GetArrayLength(Linhas) - 1 do
    begin
      Result := Result + Linhas[I] + #13#10;
    end;
  end
  else
  begin
    MsgBox('Falha ao ler o arquivo: ' + NomeDoArquivo, mbError, MB_OK);
  end;
end;

procedure SalvarStringsEmArquivo(const NomeDoArquivo, Data: String);
var
  Linhas: TArrayOfString;
begin
  // Tenho menor ideia de que char é esse, mas tá funcionando 🐱‍💻.
  Linhas := DividirString(Data, #13#10);
  if not SaveStringsToFile(NomeDoArquivo, Linhas, False) then
    MsgBox('Falha ao salvar o arquivo: ' + NomeDoArquivo, mbError, MB_OK);
end;

procedure AtualizarAppSettings(const FilePath, Endpoint, Email, Senha: String);
var
  JSONString: String;
begin
  JSONString := ObterStringDoArquivo(FilePath);

  // Valida se o json não está vazio
  if JSONString = '' then
  begin
    MsgBox('Falha ao ler o appsettings.json ou o arquivo está vazio.', mbError, MB_OK);
    Exit;
  end;
  
  // Substitui os valores padrões pelos que o usuário atribuiu
  JSONString := SubstituirString(JSONString, '"ENDPOINT_DE_AUTENTICACAO"', '"' + Endpoint + '"');
  JSONString := SubstituirString(JSONString, '"EMAIL_DO_USUARIO"', '"' + Email + '"');
  JSONString := SubstituirString(JSONString, '"SENHA_DO_USUARIO"', '"' + Senha + '"');

  // Salva o appsettings.json atualizado
  SalvarStringsEmArquivo(FilePath, JSONString);
end;

procedure CurStepChanged(CurStep: TSetupStep);
var
  CaminhoDoAppSettings: String;
  Endpoint, Email, Senha: String;
begin
  if CurStep = ssPostInstall then
  begin
    // Obtem os valores atribuídos pelo usuário durante a instalação
    Endpoint := PaginaDeCredenciaisDoUsuario.Values[0];
    Email := PaginaDeCredenciaisDoUsuario.Values[1];
    Senha := PaginaDeCredenciaisDoUsuario.Values[2];

    // Atualiza o appsettings
    CaminhoDoAppSettings := ExpandConstant('{app}\appsettings.json');
    AtualizarAppSettings(CaminhoDoAppSettings, Endpoint, Email, Senha);
  end;
end;