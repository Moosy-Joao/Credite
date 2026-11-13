using System.Text.Json;
using Credite.Credit.Api.Contratos;
using Credite.Credit.Api.Dados;
using Credite.Credit.Api.Dominio;
using Microsoft.EntityFrameworkCore;

namespace Credite.Credit.Api.Endpoints;

public static class SolicitacoesEndpoints
{
    private const string TipoEvento = "SolicitacaoCreditoCriada";

    public static IEndpointRouteBuilder MapSolicitacoesEndpoints(this IEndpointRouteBuilder rotas)
    {
        var grupo = rotas.MapGroup("/solicitacoes").WithTags("Solicitações de crédito");
        grupo.MapPost("/", Criar);
        grupo.MapGet("/", Listar);
        grupo.MapGet("/{id:guid}", ConsultarPorId);
        return rotas;
    }

    private static async Task<IResult> Criar(
        CriarSolicitacaoRequest requisicao,
        CrediteDbContext banco,
        CancellationToken cancellationToken)
    {
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
                evento.EventoId, TipoEvento, JsonSerializer.Serialize(evento)));

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
        CrediteDbContext banco,
        CancellationToken cancellationToken)
    {
        var consulta = banco.Solicitacoes.AsNoTracking();

        if (clienteId.HasValue)
            consulta = consulta.Where(solicitacao => solicitacao.ClienteId == clienteId.Value);

        var solicitacoes = await consulta
            .OrderByDescending(solicitacao => solicitacao.CriadoEm)
            .Select(solicitacao => new SolicitacaoResposta(
                solicitacao.Id, solicitacao.ClienteId, solicitacao.ValorSolicitado,
                solicitacao.Status, solicitacao.CriadoEm))
            .ToListAsync(cancellationToken);

        return Results.Ok(solicitacoes);
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
