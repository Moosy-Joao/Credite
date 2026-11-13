using Confluent.Kafka;
using Credite.Credit.Api.Dados;
using Microsoft.EntityFrameworkCore;

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
                await using var escopo = fabricaEscopos.CreateAsyncScope();
                var banco = escopo.ServiceProvider.GetRequiredService<CrediteDbContext>();
                var evento = await banco.EventosPendentes
                    .Where(item => item.PublicadoEm == null && item.Tentativas < 10)
                    .OrderBy(item => item.CriadoEm)
                    .FirstOrDefaultAsync(stoppingToken);

                if (evento is null)
                {
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    continue;
                }

                try
                {
                    await produtor.ProduceAsync(_topico, new Message<string, string>
                    {
                        Key = evento.Id.ToString(),
                        Value = evento.Conteudo
                    }, stoppingToken);

                    evento.MarcarComoPublicado();
                    logger.LogInformation("Evento {EventoId} publicado no tópico {Topico}.", evento.Id, _topico);
                }
                catch (Exception excecao)
                {
                    evento.RegistrarFalha(excecao.Message);
                    logger.LogError(excecao, "Falha ao publicar evento {EventoId}. Tentativa {Tentativa}.",
                        evento.Id, evento.Tentativas);
                }

                await banco.SaveChangesAsync(stoppingToken);

                if (evento.PublicadoEm is null)
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
}
