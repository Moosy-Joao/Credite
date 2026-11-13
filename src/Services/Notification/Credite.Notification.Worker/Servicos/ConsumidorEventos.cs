using System.Text.Json;
using Confluent.Kafka;
using Credite.Notification.Worker.Contratos;
using Credite.Notification.Worker.Dados;
using Credite.Notification.Worker.Dominio;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Credite.Notification.Worker.Servicos;

public sealed class ConsumidorEventos(
    IServiceScopeFactory fabricaEscopos,
    IProducer<string, string> produtor,
    IConfiguration configuracao,
    ILogger<ConsumidorEventos> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions OpcoesJson = new(JsonSerializerDefaults.Web);
    private readonly string _servidores = configuracao["Kafka:BootstrapServers"] ?? "localhost:9092";
    private readonly string _topico = configuracao["Kafka:TopicoSolicitacoes"] ?? "solicitacoes.credito.criada";
    private readonly string _topicoFalhas = configuracao["Kafka:TopicoFalhas"] ?? "solicitacoes.credito.criada.dlq";
    private readonly string _grupo = configuracao["Kafka:GrupoConsumidor"] ?? "credite-notification-worker";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configuracaoConsumidor = new ConsumerConfig
        {
            BootstrapServers = _servidores,
            GroupId = _grupo,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            EnableAutoOffsetStore = false
        };

        using var consumidor = new ConsumerBuilder<string, string>(configuracaoConsumidor).Build();
        consumidor.Subscribe(_topico);
        logger.LogInformation("Consumidor inscrito no tópico {Topico}.", _topico);

        while (!stoppingToken.IsCancellationRequested)
        {
            ConsumeResult<string, string> mensagem;

            try
            {
                mensagem = consumidor.Consume(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception excecao)
            {
                logger.LogError(excecao, "Falha ao receber mensagens do Kafka.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                continue;
            }

            var processada = false;

            // O offset só é confirmado depois da persistência ou do envio à DLQ.
            while (!processada && !stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessarMensagem(mensagem, stoppingToken);
                    consumidor.Commit(mensagem);
                    processada = true;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception excecao)
                {
                    logger.LogError(excecao,
                        "Falha ao processar a mensagem {Offset}. Nova tentativa em 5 segundos.",
                        mensagem.Offset);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }

        consumidor.Close();
    }

    private async Task ProcessarMensagem(
        ConsumeResult<string, string> mensagem,
        CancellationToken cancellationToken)
    {
        EventoSolicitacaoCriada? evento;

        try
        {
            evento = JsonSerializer.Deserialize<EventoSolicitacaoCriada>(mensagem.Message.Value, OpcoesJson);

            if (evento is null ||
                evento.EventoId == Guid.Empty ||
                evento.SolicitacaoId == Guid.Empty ||
                evento.ClienteId == Guid.Empty ||
                evento.ValorSolicitado < 100m ||
                evento.ValorSolicitado > 10_000m ||
                decimal.Round(evento.ValorSolicitado, 2) != evento.ValorSolicitado ||
                evento.CriadoEm == default)
            {
                throw new JsonException("O evento não contém dados válidos para processamento.");
            }
        }
        catch (JsonException excecao)
        {
            logger.LogWarning(excecao, "Evento inválido enviado para a dead-letter topic.");
            await produtor.ProduceAsync(_topicoFalhas, new Message<string, string>
            {
                Key = mensagem.Message.Key,
                Value = mensagem.Message.Value
            }, cancellationToken);
            return;
        }

        await using var escopo = fabricaEscopos.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<NotificacoesDbContext>();

        var eventoJaProcessado = await banco.Notificacoes
            .AnyAsync(notificacao => notificacao.EventoId == evento.EventoId, cancellationToken);

        if (eventoJaProcessado)
        {
            logger.LogInformation("Evento {EventoId} já processado. Ignorando duplicidade.", evento.EventoId);
            return;
        }

        var notificacao = new Notificacao(
            evento.EventoId,
            evento.SolicitacaoId,
            evento.ClienteId,
            $"Sua solicitação de crédito no valor de R$ {evento.ValorSolicitado:N2} foi registrada e está em análise.");

        banco.Notificacoes.Add(notificacao);
        await banco.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Notificação {NotificacaoId} registrada para a solicitação {SolicitacaoId}.",
            notificacao.Id, notificacao.SolicitacaoId);
    }
}
