using Credite.Web.Models;

namespace Credite.Web.Servicos;

public interface ICrediteApiClient
{
    Task<ClienteApiDto> CadastrarClienteAsync(
        CadastrarClienteApiRequest requisicao,
        CancellationToken cancellationToken);

    Task<ClienteApiDto?> ConsultarClienteAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<SolicitacaoApiDto> CriarSolicitacaoAsync(
        CriarSolicitacaoApiRequest requisicao,
        CancellationToken cancellationToken);

    Task<ListaPaginadaApiDto<SolicitacaoApiDto>> ListarSolicitacoesAsync(
        Guid? clienteId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken);

    Task<SolicitacaoApiDto?> ConsultarSolicitacaoAsync(
        Guid id,
        CancellationToken cancellationToken);
}
