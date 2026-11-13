using System.Globalization;
using System.Text;
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
using Npgsql;

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

            // O offset só é confirmado depois da persistência ou do envio confirmado à DLQ.
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
        EventoSolicitacaoCriada? evento = null;
        string? motivoInvalidez = null;

        try
        {
            var conteudo = mensagem.Message?.Value;
            if (string.IsNullOrWhiteSpace(conteudo))
                throw new JsonException("O payload está vazio ou é nulo.");

            evento = JsonSerializer.Deserialize<EventoSolicitacaoCriada>(conteudo, OpcoesJson);

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
            motivoInvalidez = excecao.Message;
        }

        if (motivoInvalidez is not null)
        {
            await EncaminharParaDeadLetter(mensagem, motivoInvalidez, cancellationToken);
            return;
        }

        await using var escopo = fabricaEscopos.CreateAsyncScope();
        var banco = escopo.ServiceProvider.GetRequiredService<NotificacoesDbContext>();

        var eventoJaProcessado = await banco.Notificacoes
            .AnyAsync(notificacao => notificacao.EventoId == evento!.EventoId, cancellationToken);

        if (eventoJaProcessado)
        {
            logger.LogInformation("Evento {EventoId} já processado. Ignorando duplicidade.", evento!.EventoId);
            return;
        }

        var notificacao = new Notificacao(
            evento!.EventoId,
            evento.SolicitacaoId,
            evento.ClienteId,
            $"Sua solicitação de crédito no valor de R$ {evento.ValorSolicitado:N2} foi registrada e está em análise.");

        banco.Notificacoes.Add(notificacao);

        try
        {
            await banco.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException excecao) when (EhEventoDuplicado(excecao))
        {
            // A constraint única é a proteção definitiva contra duas entregas concorrentes.
            logger.LogInformation("Evento {EventoId} já persistido por outro processamento.", evento.EventoId);
        }

        logger.LogInformation("Notificação {NotificacaoId} registrada para a solicitação {SolicitacaoId}.",
            notificacao.Id, notificacao.SolicitacaoId);
    }

    private async Task EncaminharParaDeadLetter(
        ConsumeResult<string, string> mensagem,
        string motivo,
        CancellationToken cancellationToken)
    {
        var cabecalhos = new Headers
        {
            new("credite-topico-origem", Encoding.UTF8.GetBytes(mensagem.Topic)),
            new("credite-particao-origem", Encoding.UTF8.GetBytes(mensagem.Partition.Value.ToString(CultureInfo.InvariantCulture))),
            new("credite-offset-origem", Encoding.UTF8.GetBytes(mensagem.Offset.Value.ToString(CultureInfo.InvariantCulture))),
            new("credite-falha-em", Encoding.UTF8.GetBytes(DateTimeOffset.UtcNow.ToString("O", CultureInfo.InvariantCulture))),
            new("credite-motivo", Encoding.UTF8.GetBytes(motivo.Length <= 500 ? motivo : motivo[..500]))
        };

        await produtor.ProduceAsync(_topicoFalhas, new Message<string, string>
        {
            Key = mensagem.Message?.Key,
            Value = mensagem.Message?.Value,
            Headers = cabecalhos
        }, cancellationToken);

        logger.LogWarning(
            "Mensagem inválida encaminhada à DLQ. Tópico {Topico}, partição {Particao}, offset {Offset}.",
            mensagem.Topic, mensagem.Partition.Value, mensagem.Offset.Value);
    }

    private static bool EhEventoDuplicado(DbUpdateException excecao) =>
        excecao.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "ix_notificacoes_evento_id"
        };
}
