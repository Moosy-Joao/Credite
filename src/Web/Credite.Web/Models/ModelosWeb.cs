using System.ComponentModel.DataAnnotations;

namespace Credite.Web.Models;

public sealed class ClienteFormularioViewModel
{
    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um endereço de e-mail válido.")]
    [StringLength(254)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;
}

public sealed class ClienteConsultaViewModel
{
    [Display(Name = "Identificador do cliente")]
    public string Id { get; set; } = string.Empty;

    public ClienteApiDto? Cliente { get; set; }

    public string? Mensagem { get; set; }
}

public sealed class SolicitacaoFormularioViewModel
{
    [Required(ErrorMessage = "Informe o identificador do cliente.")]
    [Display(Name = "ID do cliente")]
    public Guid ClienteId { get; set; }

    [Range(typeof(decimal), "100", "10000",
        ErrorMessage = "O valor deve estar entre R$ 100,00 e R$ 10.000,00.")]
    [Display(Name = "Valor solicitado (R$)")]
    public decimal ValorSolicitado { get; set; }
}

public sealed class SolicitacoesListaViewModel
{
    [Display(Name = "Filtrar por ID do cliente")]
    public Guid? ClienteId { get; set; }

    public int Pagina { get; set; }
    public int TamanhoPagina { get; set; }
    public int TotalRegistros { get; set; }
    public int TotalPaginas { get; set; }
    public IReadOnlyList<SolicitacaoApiDto> Itens { get; set; } = [];
}

public sealed record ClienteApiDto(Guid Id, string Nome, string Email, DateTimeOffset CriadoEm);

public sealed record SolicitacaoApiDto(
    Guid Id,
    Guid ClienteId,
    decimal ValorSolicitado,
    string Status,
    DateTimeOffset CriadoEm);

public sealed record ListaPaginadaApiDto<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalRegistros,
    int TotalPaginas);

public sealed record CadastrarClienteApiRequest(string Nome, string Email);

public sealed record CriarSolicitacaoApiRequest(Guid ClienteId, decimal ValorSolicitado);
