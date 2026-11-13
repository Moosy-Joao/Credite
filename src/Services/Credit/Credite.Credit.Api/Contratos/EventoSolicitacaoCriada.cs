namespace Credite.Credit.Api.Contratos;

// Este contrato também existe no Notification Worker.
public sealed record EventoSolicitacaoCriada(
    Guid EventoId,
    Guid SolicitacaoId,
    Guid ClienteId,
    decimal ValorSolicitado,
    DateTimeOffset CriadoEm);
