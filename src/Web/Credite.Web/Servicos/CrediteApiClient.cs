using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Credite.Web.Models;

namespace Credite.Web.Servicos;

public sealed class CrediteApiClient(HttpClient cliente) : ICrediteApiClient
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    public Task<ClienteApiDto> CadastrarClienteAsync(
        CadastrarClienteApiRequest requisicao,
        CancellationToken cancellationToken) =>
        EnviarAsync<ClienteApiDto>(
            () => cliente.PostAsJsonAsync("clientes", requisicao, OpcoesJson, cancellationToken),
            cancellationToken);

    public async Task<ClienteApiDto?> ConsultarClienteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        using var resposta = await cliente.GetAsync($"clientes/{id}", cancellationToken);
        if (resposta.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await LerRespostaAsync<ClienteApiDto>(resposta, cancellationToken);
    }

    public Task<SolicitacaoApiDto> CriarSolicitacaoAsync(
        CriarSolicitacaoApiRequest requisicao,
        CancellationToken cancellationToken) =>
        EnviarAsync<SolicitacaoApiDto>(
            () => cliente.PostAsJsonAsync("solicitacoes", requisicao, OpcoesJson, cancellationToken),
            cancellationToken);

    public async Task<ListaPaginadaApiDto<SolicitacaoApiDto>> ListarSolicitacoesAsync(
        Guid? clienteId,
        int pagina,
        int tamanhoPagina,
        CancellationToken cancellationToken)
    {
        var consulta = $"solicitacoes?pagina={pagina}&tamanhoPagina={tamanhoPagina}";
        if (clienteId.HasValue)
            consulta += $"&clienteId={clienteId.Value}";

        using var resposta = await cliente.GetAsync(consulta, cancellationToken);
        return await LerRespostaAsync<ListaPaginadaApiDto<SolicitacaoApiDto>>(
            resposta, cancellationToken);
    }

    public async Task<SolicitacaoApiDto?> ConsultarSolicitacaoAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        using var resposta = await cliente.GetAsync($"solicitacoes/{id}", cancellationToken);
        if (resposta.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await LerRespostaAsync<SolicitacaoApiDto>(resposta, cancellationToken);
    }

    private async Task<T> EnviarAsync<T>(
        Func<Task<HttpResponseMessage>> enviar,
        CancellationToken cancellationToken)
    {
        using var resposta = await enviar();
        return await LerRespostaAsync<T>(resposta, cancellationToken);
    }

    private static async Task<T> LerRespostaAsync<T>(
        HttpResponseMessage resposta,
        CancellationToken cancellationToken)
    {
        if (!resposta.IsSuccessStatusCode)
        {
            var mensagem = await LerMensagemErroAsync(resposta, cancellationToken);
            throw new CrediteApiException(resposta.StatusCode, mensagem);
        }

        var resultado = await resposta.Content.ReadFromJsonAsync<T>(
            OpcoesJson, cancellationToken);

        return resultado ?? throw new InvalidOperationException(
            "A Credit API retornou uma resposta vazia ou inválida.");
    }

    private static async Task<string> LerMensagemErroAsync(
        HttpResponseMessage resposta,
        CancellationToken cancellationToken)
    {
        var conteudo = await resposta.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(conteudo))
            return $"A Credit API retornou HTTP {(int)resposta.StatusCode}.";

        try
        {
            using var documento = JsonDocument.Parse(conteudo);
            var raiz = documento.RootElement;

            foreach (var propriedade in new[] { "mensagem", "detail", "title" })
            {
                if (raiz.TryGetProperty(propriedade, out var valor) &&
                    valor.ValueKind == JsonValueKind.String &&
                    !string.IsNullOrWhiteSpace(valor.GetString()))
                {
                    return valor.GetString()!;
                }
            }

            if (raiz.TryGetProperty("errors", out var erros) &&
                erros.ValueKind == JsonValueKind.Object)
            {
                var mensagens = erros.EnumerateObject()
                    .SelectMany(erro => erro.Value.ValueKind == JsonValueKind.Array
                        ? erro.Value.EnumerateArray()
                            .Where(item => item.ValueKind == JsonValueKind.String)
                            .Select(item => item.GetString())
                        : [])
                    .Where(mensagem => !string.IsNullOrWhiteSpace(mensagem));

                var mensagemValidacao = string.Join(" ", mensagens);
                if (!string.IsNullOrWhiteSpace(mensagemValidacao))
                    return mensagemValidacao;
            }
        }
        catch (JsonException)
        {
            // A resposta pode não estar no formato Problem Details.
        }

        return $"A Credit API retornou HTTP {(int)resposta.StatusCode}.";
    }
}
