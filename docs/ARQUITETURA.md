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
- Trata conflitos de e-mail pela verificação antecipada e pela constraint única do PostgreSQL.
- Pagina consultas de solicitações: página padrão 1, tamanho padrão 20 e máximo 100.
- Registra a solicitação e o evento Outbox na mesma transação.
- Publica eventos pendentes no Kafka.
- Não consulta o banco do Notification.

## Notification Worker

- Consome eventos do Kafka.
- Persiste notificações no banco próprio.
- Usa o identificador do evento para idempotência.
- Trata a constraint única de `evento_id` como proteção definitiva contra entregas concorrentes.
- Confirma o offset somente após persistência ou após envio confirmado à DLQ.
- Encaminha mensagens inválidas com headers de tópico, partição, offset, horário e motivo.

## Persistência

Cada microsserviço possui seu próprio banco PostgreSQL. No ambiente local, os bancos são criados na mesma instância para reduzir a infraestrutura necessária. Não existem tabelas, migrations, DbContexts ou transações compartilhadas.

## Outbox e entrega

A solicitação e o evento pendente são persistidos na mesma transação. O publicador seleciona cada registro com `FOR UPDATE SKIP LOCKED` dentro de uma transação, mantendo o bloqueio até salvar o resultado da publicação. Isso impede dois publicadores de selecionar simultaneamente a mesma linha. O timeout de publicação Kafka é limitado a 15 segundos.

A entrega é **at-least-once**, não exactly-once: se Kafka aceitar a mensagem e o processo cair antes de persistir `publicado_em`, o evento poderá ser publicado novamente. Por isso, o Notification aplica idempotência no banco e trata a constraint única como proteção definitiva.

O bloqueio transacional é adequado ao volume didático atual. Ele mantém uma transação PostgreSQL aberta durante o envio Kafka, limitado pelo timeout do produtor. Se volume ou latência crescerem, a evolução natural será reivindicação por lease persistido e processamento em lotes.

A Outbox realiza até dez tentativas. Eventos que esgotarem as tentativas permanecem registrados para inspeção manual.

## Saúde e prontidão

- `/health/live` informa se o processo da API está ativo.
- `/health` verifica PostgreSQL e Kafka e retorna 503 quando uma dependência obrigatória está indisponível.
- O Worker não expõe HTTP no MVP; seu estado é acompanhado pelo processo e pelos logs.

## Configuração local

As credenciais versionadas em `appsettings.json`, nas fábricas de design-time e no Docker Compose são exclusivas para desenvolvimento local descartável. Os processos .NET e as fábricas do EF Core aceitam sobrescrita da conexão pela variável de ambiente `ConnectionStrings__BancoDados`. Nenhum valor de desenvolvimento deve ser reutilizado em produção.

## Tecnologias

- .NET 10 e C# 14.
- ASP.NET Core Minimal APIs e Scalar.
- EF Core 10 e PostgreSQL.
- Apache Kafka e Confluent.Kafka.
- Worker Service e xUnit.
- Docker Compose.

## Limites

Autenticação, interface Web, API Gateway, Redis, MongoDB, terceiro microsserviço e Saga não fazem parte do MVP. Os dados e regras de crédito são inteiramente fictícios.
