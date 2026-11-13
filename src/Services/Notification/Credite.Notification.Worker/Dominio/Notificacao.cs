namespace Credite.Notification.Worker.Dominio;

public sealed class Notificacao
{
    private Notificacao() { }

    public Notificacao(Guid eventoId, Guid solicitacaoId, Guid clienteId, string mensagem)
    {
        Id = Guid.NewGuid();
        EventoId = eventoId;
        SolicitacaoId = solicitacaoId;
        ClienteId = clienteId;
        Mensagem = mensagem;
        CriadoEm = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid EventoId { get; private set; }
    public Guid SolicitacaoId { get; private set; }
    public Guid ClienteId { get; private set; }
    public string Mensagem { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
}
