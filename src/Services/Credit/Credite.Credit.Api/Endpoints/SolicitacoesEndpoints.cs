using System.Text.Json;
using Credite.Credit.Api.Contratos;
using Credite.Credit.Api.Dados;
using Credite.Credit.Api.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Credite.Credit.Api.Endpoints;

public static class SolicitacoesEndpoints
{
    private const string TipoEvento = "SolicitacaoCreditoCriada";
    private const int PaginaPadrao = 1;
    private const int TamanhoPaginaPadrao = 20;
    private const int TamanhoPaginaMaximo = 100;
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);

    public static IEndpointRouteBuilder MapSolicitacoesEndpoints(this IEndpointRouteBuilder rotas)
    {
        var grupo = rotas.MapGroup("/solicitacoes").WithTags("Solicitações de crédito");
        grupo.MapPost("/", Criar);
        grupo.MapGet("/", Listar);
        grupo.MapGet("/{id:guid}", ConsultarPorId);
        return rotas;
    }

    private static async Task<IResult> Criar(
        CriarSolicitacaoRequest? requisicao,
        CrediteDbContext banco,
        CancellationToken cancellationToken)
    {
        if (requisicao is null)
            return Results.BadRequest(new { mensagem = "O corpo da solicitação é obrigatório." });

        var clienteExiste = await banco.Clientes.AnyAsync(
            cliente => cliente.Id == requisicao.ClienteId, cancellationToken);

        if (!clienteExiste)
            return Results.NotFound(new { mensagem = "Cliente não encontrado." });

        try
        {
            var solicitacao = new SolicitacaoCredito(requisicao.ClienteId, requisicao.ValorSolicitado);
            var evento = new EventoSolicitacaoCriada(
                Guid.NewGuid(), solicitacao.Id, solicitacao.ClienteId,
                solicitacao.ValorSolicitado, solicitacao.CriadoEm);

            banco.Solicitacoes.Add(solicitacao);
            banco.EventosPendentes.Add(new EventoPendente(
                evento.EventoId, TipoEvento, JsonSerializer.Serialize(evento, OpcoesJson)));

            // Solicitação e evento são gravados na mesma transação do PostgreSQL.
            await banco.SaveChangesAsync(cancellationToken);

            return Results.Created($"/solicitacoes/{solicitacao.Id}", SolicitacaoResposta.De(solicitacao));
        }
        catch (ArgumentException excecao)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["solicitacao"] = [excecao.Message]
            });
        }
    }

    private static async Task<IResult> Listar(
        Guid? clienteId,
        int? pagina,
        int? tamanhoPagina,
        CrediteDbContext banco,
        CancellationToken cancellationToken)
    {
        var numeroPagina = pagina ?? PaginaPadrao;
        var tamanho = tamanhoPagina ?? TamanhoPaginaPadrao;

        if (numeroPagina < 1 || tamanho < 1 || tamanho > TamanhoPaginaMaximo ||
            numeroPagina > int.MaxValue / tamanho)
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                ["paginacao"] = [$"A página deve ser maior que zero e o tamanho deve estar entre 1 e {TamanhoPaginaMaximo}."]
            });
        }

        var consulta = banco.Solicitacoes.AsNoTracking();

        if (clienteId.HasValue)
            consulta = consulta.Where(solicitacao => solicitacao.ClienteId == clienteId.Value);

        var totalRegistros = await consulta.CountAsync(cancellationToken);
        var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)tamanho);

        var itens = await consulta
            .OrderByDescending(solicitacao => solicitacao.CriadoEm)
            .ThenBy(solicitacao => solicitacao.Id)
            .Skip((numeroPagina - 1) * tamanho)
            .Take(tamanho)
            .Select(solicitacao => new SolicitacaoResposta(
                solicitacao.Id, solicitacao.ClienteId, solicitacao.ValorSolicitado,
                solicitacao.Status, solicitacao.CriadoEm))
            .ToListAsync(cancellationToken);

        return Results.Ok(new ListaPaginada<SolicitacaoResposta>(
            itens, numeroPagina, tamanho, totalRegistros, totalPaginas));
    }

    private static async Task<IResult> ConsultarPorId(
        Guid id,
        CrediteDbContext banco,
        CancellationToken cancellationToken)
    {
        var solicitacao = await banco.Solicitacoes.AsNoTracking()
            .FirstOrDefaultAsync(solicitacao => solicitacao.Id == id, cancellationToken);

        return solicitacao is null
            ? Results.NotFound()
            : Results.Ok(SolicitacaoResposta.De(solicitacao));
    }
}

public sealed record CriarSolicitacaoRequest(Guid ClienteId, decimal ValorSolicitado);

public sealed record SolicitacaoResposta(
    Guid Id, Guid ClienteId, decimal ValorSolicitado, string Status, DateTimeOffset CriadoEm)
{
    public static SolicitacaoResposta De(SolicitacaoCredito solicitacao) =>
        new(solicitacao.Id, solicitacao.ClienteId, solicitacao.ValorSolicitado,
            solicitacao.Status, solicitacao.CriadoEm);
}

public sealed record ListaPaginada<T>(
    IReadOnlyList<T> Itens,
    int Pagina,
    int TamanhoPagina,
    int TotalRegistros,
    int TotalPaginas);
