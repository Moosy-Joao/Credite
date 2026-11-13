# ADR-0001 — Arquitetura mínima com dois microsserviços

- Estado: aceita.
- Data: 2026-11-13.
- Contexto: simplificação do Credite para priorizar aprendizado prático e conclusão do projeto.

## Contexto

A arquitetura anterior previa quatro microsserviços, interface Web, múltiplos bancos e diversos projetos por camada. Essa estrutura aumentou o custo de implementação antes de existir um fluxo funcional.

O objetivo educacional permanece aprender microsserviços com C# e .NET, incluindo comunicação assíncrona e independência de dados.

## Decisão

Manter dois microsserviços:

| Serviço | Responsabilidade | Persistência |
|---|---|---|
| Credit API | Clientes fictícios e solicitações de crédito | PostgreSQL próprio |
| Notification Worker | Consumo de eventos e registro de notificações | PostgreSQL próprio |

Apache Kafka será o meio de comunicação assíncrona. Cada serviço será responsável pelos próprios dados. No ambiente local, ambos os bancos podem compartilhar a mesma instância PostgreSQL, desde que sejam bancos separados.

Cada microsserviço terá inicialmente um único projeto executável. Novos projetos, bibliotecas e camadas só serão criados quando uma necessidade concreta justificar a separação.

## Removido do escopo atual

- Identity e autenticação.
- Customer como microsserviço independente.
- Interface Web.
- API Gateway e BFF.
- Redis e MongoDB.
- Projetos separados de Domain, Application e Infrastructure.
- Comunicação HTTP entre microsserviços.
- Saga e outras coordenações distribuídas.

## Confiabilidade

A primeira integração será deliberadamente simples. Depois que o fluxo básico estiver funcionando, serão estudados e implementados Outbox, idempotência, retry limitado e dead-letter topic. A publicação direta após persistência tem risco de dual-write e não será apresentada como garantia de produção.

## Consequências

A arquitetura fica menor, mais fácil de executar localmente e adequada para aprender os fundamentos de dois serviços independentes. Em contrapartida, algumas garantias de confiabilidade serão introduzidas em etapas posteriores.

## Histórico

Esta decisão substitui a arquitetura de quatro serviços definida anteriormente no ADR-0014. Os documentos e implementações anteriores permanecem recuperáveis no histórico Git.
