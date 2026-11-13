namespace Credite.Notification.Worker.Contratos;

// Deve permanecer compatível com o contrato publicado pelo Credit API.
public sealed record EventoSolicitacaoCriada(
    Guid EventoId,
    Guid SolicitacaoId,
    Guid ClienteId,
    decimal ValorSolicitado,
    DateTimeOffset CriadoEm);
