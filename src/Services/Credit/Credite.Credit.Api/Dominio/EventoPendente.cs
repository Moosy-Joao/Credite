namespace Credite.Credit.Api.Dominio;

public sealed class EventoPendente
{
    private EventoPendente() { }

    public EventoPendente(Guid id, string tipo, string conteudo)
    {
        Id = id;
        Tipo = tipo;
        Conteudo = conteudo;
        CriadoEm = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Tipo { get; private set; } = string.Empty;
    public string Conteudo { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
    public DateTimeOffset? PublicadoEm { get; private set; }
    public int Tentativas { get; private set; }
    public string? UltimoErro { get; private set; }

    public void MarcarComoPublicado()
    {
        PublicadoEm = DateTimeOffset.UtcNow;
        UltimoErro = null;
    }

    public void RegistrarFalha(string mensagem)
    {
        Tentativas++;
        UltimoErro = mensagem.Length <= 500 ? mensagem : mensagem[..500];
    }
}
