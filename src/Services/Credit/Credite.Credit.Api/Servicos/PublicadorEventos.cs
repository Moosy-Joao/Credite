using Confluent.Kafka;
using Credite.Credit.Api.Dados;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Credite.Credit.Api.Servicos;

public sealed class PublicadorEventos(
    IServiceScopeFactory fabricaEscopos,
    IProducer<string, string> produtor,
    IConfiguration configuracao,
    ILogger<PublicadorEventos> logger) : BackgroundService
{
    private readonly string _topico = configuracao["Kafka:TopicoSolicitacoes"]
        ?? "solicitacoes.credito.criada";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Publicador da Outbox iniciado.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var encontrouEvento = await PublicarProximoEvento(stoppingToken);

                if (!encontrouEvento)
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception excecao)
            {
                logger.LogError(excecao, "Erro ao processar a Outbox.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private async Task<bool> PublicarProximoEvento(CancellationToken cancellationToken)
    {
        await using var escopo = fabricaEscopos.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<CrediteDbContext>();
        await using var transacao = await banco.Database.BeginTransactionAsync(cancellationToken);

        // O bloqueio de linha e SKIP LOCKED impedem dois publicadores de reivindicar o mesmo evento.
        // A transação permanece aberta durante o envio; o timeout do produtor limita sua duração.
        var conexao = banco.Database.GetDbConnection();
        await using var comando = conexao.CreateCommand();
        comando.Transaction = banco.Database.CurrentTransaction!.GetDbTransaction();
        comando.CommandText = """
            SELECT id
            FROM credito.eventos_pendentes
            WHERE publicado_em IS NULL
              AND tentativas < 10
            ORDER BY criado_em, id
            LIMIT 1
            FOR UPDATE SKIP LOCKED;
            """;

        var resultado = await comando.ExecuteScalarAsync(cancellationToken);

        if (resultado is null || resultado is DBNull)
        {
            await transacao.CommitAsync(cancellationToken);
            return false;
        }

        var eventoId = (Guid)resultado;
        var evento = await banco.EventosPendentes
            .SingleAsync(item => item.Id == eventoId, cancellationToken);

        try
        {
            await produtor.ProduceAsync(_topico, new Message<string, string>
            {
                Key = evento.Id.ToString(),
                Value = evento.Conteudo
            }, cancellationToken);

            evento.MarcarComoPublicado();
            logger.LogInformation("Evento {EventoId} publicado no tópico {Topico}.", evento.Id, _topico);
        }
        catch (Exception excecao) when (excecao is not OperationCanceledException)
        {
            evento.RegistrarFalha(excecao.Message);
            logger.LogError(excecao, "Falha ao publicar evento {EventoId}. Tentativa {Tentativa}.",
                evento.Id, evento.Tentativas);
        }

        await banco.SaveChangesAsync(cancellationToken);
        await transacao.CommitAsync(cancellationToken);
        return true;
    }
}
