using System.Net.Mail;

namespace Credite.Credit.Api.Dominio;

public sealed class Cliente
{
    private Cliente() { }

    public Cliente(string nome, string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(nome);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var nomeNormalizado = nome.Trim();
        var emailNormalizado = email.Trim();

        if (nomeNormalizado.Length < 2 || nomeNormalizado.Length > 100)
            throw new ArgumentException("O nome deve ter entre 2 e 100 caracteres.", nameof(nome));

        if (emailNormalizado.Length > 254 ||
            !MailAddress.TryCreate(emailNormalizado, out var endereco) ||
            !string.Equals(endereco.Address, emailNormalizado, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Informe um endereço de e-mail válido.", nameof(email));

        Id = Guid.NewGuid();
        Nome = nomeNormalizado;
        Email = emailNormalizado.ToLowerInvariant();
        CriadoEm = DateTimeOffset.UtcNow;
    }

    public Guid Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public DateTimeOffset CriadoEm { get; private set; }
}
