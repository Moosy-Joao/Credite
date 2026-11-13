namespace Credite.Credit.Api.Dominio;

public sealed class SolicitacaoCredito
{
    private SolicitacaoCredito() { }

    public SolicitacaoCredito(Guid clienteId, decimal valorSolicitado)
    {
        if (clienteId == Guid.Empty)
            throw new ArgumentException("O cliente informado é inválido.", nameof(clienteId));

        if (valorSolicitado < 100m || valorSolicitado > 10_000m)
            throw new ArgumentOutOfRangeException(nameof(valorSolicitado), "O valor deve estar entre R$ 100,00 e R$ 10.000,00.");

        if (decimal.Round(valorSolicitado, 2) != valorSolicitado)
            throw new ArgumentException("O valor deve possuir no máximo duas casas decimais.", nameof(valorSolicitado));

        Id = Guid.NewGuid();
        ClienteId = clienteId;
        ValorSolicitado = valorSolicitado;
        Status = "EmAnalise";
        CriadoEm = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public Guid ClienteId { get; private set; }
    public decimal ValorSolicitado { get; private set; }
    public string Status { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
}
