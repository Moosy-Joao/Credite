# Arquitetura do Credite

## Visão geral

O Credite possui dois microsserviços independentes:

1. Credit API: cadastra clientes fictícios, registra solicitações e publica eventos.
2. Notification Worker: consome eventos e registra notificações em seu próprio banco.

## Diagrama

~~~mermaid
flowchart LR
    Cliente[Cliente HTTP] --> API[Credit API]
    API --> BancoCredito[(PostgreSQL credito)]
    API --> Outbox[Outbox]
    Outbox --> Kafka[(Apache Kafka)]
    Kafka --> Worker[Notification Worker]
    Worker --> BancoNotificacao[(PostgreSQL notificacoes)]
~~~

## Credit API

- É responsável por clientes e solicitações.
- Valida os dados de entrada e aplica limites demonstrativos.
- Registra a solicitação e o evento Outbox na mesma transação.
- Publica eventos pendentes no Kafka.
- Não consulta o banco do Notification.

## Notification Worker

- Consome eventos do Kafka.
- Persiste notificações no banco próprio.
- Usa o identificador do evento para idempotência.
- Confirma o offset somente após persistência.
- Encaminha mensagens inválidas para uma dead-letter topic.

## Persistência

Cada microsserviço possui seu próprio banco PostgreSQL. No ambiente local, os bancos são criados na mesma instância para reduzir a infraestrutura necessária. Não existem tabelas, migrations, DbContexts ou transações compartilhadas.

## Outbox e entrega

A solicitação e o evento pendente são persistidos na mesma transação. Um serviço em segundo plano publica os eventos pendentes. Isso evita perder o evento quando a aplicação falha depois de salvar a solicitação.

A publicação pode ocorrer mais de uma vez se o Kafka aceitar a mensagem e a aplicação falhar antes de marcar a Outbox como publicada. Por isso, o Notification usa eventoId único e processamento idempotente.

A Outbox realiza até dez tentativas. Eventos que esgotarem as tentativas permanecem registrados para inspeção manual.

## Tecnologias

- .NET 10 e C# 14.
- ASP.NET Core Minimal APIs e Scalar.
- EF Core 10 e PostgreSQL.
- Apache Kafka e Confluent.Kafka.
- Worker Service e xUnit.
- Docker Compose.

## Limites

Autenticação, interface Web, API Gateway, Redis, MongoDB, terceiro microsserviço e Saga não fazem parte do MVP. Os dados e regras de crédito são inteiramente fictícios.
