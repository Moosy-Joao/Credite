# ADR-0014 — Microsserviços, persistência por serviço e mensageria

- Estado: aceita.
- Data: 2026-11-07.
- Responsável: responsável pelo projeto.
- Substitui: ADR-0001 (banco oficial), ADR-0005 (arquitetura de implantação nos pontos conflitantes), ADR-0012 (monólito modular como arquitetura-alvo) e ADR-0013 (tecnologias apenas em laboratório nos pontos conflitantes).

## Contexto

O Credite é um projeto educacional de Back-End C#/.NET. A arquitetura anterior previa monólito modular e SQL Server. O responsável aprovou a mudança para quatro microsserviços, com ownership de dados, persistência poliglota e mensageria.

A decisão estabelece os limites e tecnologias principais, sem antecipar escolhas ainda não aprovadas sobre autenticação distribuída, comunicação síncrona, integração da interface ou Gateway.

## Decisão

Adotar quatro microsserviços, cada um com responsabilidade e persistência próprias.

### 1. Serviços e bancos

| Serviço | Responsabilidade | Banco |
|---|---|---|
| Identity | Contas internas, credenciais, perfis e identidade | PostgreSQL exclusivo |
| Customer | Cadastro e manutenção de clientes fictícios | PostgreSQL exclusivo |
| Credit | Solicitações, estados, simulações demonstrativas, avaliações, histórico e Outbox | PostgreSQL exclusivo |
| Notification | Consumo de eventos e processamento de notificações demonstrativas | MongoDB exclusivo |

Identity, Customer e Credit utilizarão EF Core 10 com Npgsql em versões estáveis compatíveis com .NET 10. Cada serviço possuirá DbContext, configuração, connection string e migrations independentes.

Notification utilizará o driver oficial MongoDB .NET em versão estável compatível. Não introduzir PostgreSQL no Notification nem MongoDB nos demais serviços.

### 2. Ownership

- Cada serviço é autoridade exclusiva sobre seus dados.
- Nenhum serviço consulta ou altera diretamente o banco de outro.
- Não compartilhar tabelas, schemas, DbContexts, migrations, entidades de persistência ou transações.
- Referências entre serviços usam identificadores e contratos explícitos.
- Transações ACID são locais ao banco de cada serviço.
- Migrations são versionadas, explícitas e revisáveis; não executar automaticamente em produção.

### 3. Fluxo Credit → Kafka → Notification

Apache Kafka será utilizado no fluxo assíncrono aprovado.

Credit implementará Transactional Outbox: a alteração de negócio e o evento pendente serão gravados na mesma transação PostgreSQL. Um publicador recuperável enviará os eventos pendentes ao Kafka e atualizará o estado da Outbox após confirmação.

Notification consumirá os eventos e persistirá o processamento no MongoDB. O consumidor deverá ser idempotente, usando identificador estável e deduplicação persistente. Mensagens podem ser entregues mais de uma vez.

A política deverá contemplar entrega pelo menos uma vez, confirmação coerente de publicação/consumo, retry limitado apenas para falhas transitórias, dead-letter topic após esgotamento, observabilidade e procedimento de redrive.

Não prometer exactly-once ponta a ponta. Confluent.Kafka é o cliente .NET preferencial para avaliação, desde que versão estável e compatibilidade com .NET 10 sejam verificadas antes da implementação.

Nomes, versões, envelope, payload, tópicos, partições, chaves e retenção dos eventos precisam ser definidos antes da implementação dos contratos.

### 4. Interface administrativa

A interface .NET existente permanece cliente da plataforma e não constitui quinto microsserviço de negócio. Não terá acesso direto aos bancos nem regras de domínio.

A forma de integração com Identity, Customer e Credit permanece pendente. Esta ADR não escolhe BFF, chamadas diretas, API Gateway ou YARP.

### 5. Segurança e comunicação

Identity é dono dos dados de identidade. Esta ADR não define protocolo de autenticação, emissão/validação de tokens, sessão, propagação de identidade, autorização distribuída ou comunicação síncrona.

Não presumir compartilhamento dos cookies dos hosts antigos nem introduzir JWT, OAuth 2.0, OpenID Connect ou Gateway sem decisão específica. Esses assuntos serão tratados em ADR-0015, após confirmação explícita do responsável.

### 6. Convenções de nomenclatura

Solução, projetos, namespaces e pastas arquiteturais em inglês. Identificadores internos C# em português, obedecendo às convenções oficiais: PascalCase para tipos/métodos/propriedades, camelCase para parâmetros/variáveis, prefixo I para interfaces e sufixo Async para métodos assíncronos. Tabelas/colunas em português snake_case. Testes, documentação e commits em português. Tipos e APIs oficiais do framework permanecem em inglês.

## Alternativas consideradas

- Manter monólito modular com SQL Server: decisão anterior, substituída.
- Banco compartilhado: rejeitado por contrariar ownership.
- PostgreSQL também para Notification: rejeitado; Notification usa MongoDB.
- MongoDB para todos: rejeitado; Identity, Customer e Credit usam modelo relacional.
- RabbitMQ para o fluxo aprovado: não escolhido; Apache Kafka foi selecionado.
- Publicação direta sem Outbox: rejeitada pelo risco de dual-write.
- Exactly-once ponta a ponta: não prometido; adotar entrega pelo menos uma vez e idempotência.
- Definir autenticação e Gateway nesta ADR: adiado para ADR-0015, mediante confirmação.

## Consequências

**Benefícios:** ownership explícito, prática concreta de PostgreSQL/MongoDB/Kafka/Outbox/idempotência e possibilidade de evolução independente.

**Trade-offs:** maior complexidade local, consistência eventual, contratos versionados, observabilidade distribuída e maior custo operacional.

## Riscos e mitigação

- Acesso cruzado a dados: impedir dependências diretas e testar limites.
- Evento perdido entre commit e publicação: Outbox e publicador recuperável.
- Duplicidade: consumidor idempotente.
- Retry infinito: limite explícito e dead-letter topic.
- Outbox acumulada: métricas e procedimento de recuperação.
- Contrato incompatível: versionamento e testes de contrato.
- Complexidade excessiva: escopo fictício e implementação em fatias pequenas.

## Pendências para ADR-0015

Após confirmação explícita:
- protocolo de autenticação/autorização;
- integração da interface administrativa;
- comunicação síncrona entre serviços;
- contratos REST e políticas de acesso;
- necessidade/responsabilidade de API Gateway;
- contratos, envelope, tópicos, chaves e versionamento dos eventos.

## Referências

- [Microsoft — Microservices architecture style](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/microservices)
- [Microsoft — Data sovereignty per microservice](https://learn.microsoft.com/dotnet/architecture/microservices/architect-microservice-container-applications/data-sovereignty-per-microservice)
- [Npgsql — EF Core Provider](https://www.npgsql.org/efcore/)
- [MongoDB — .NET/C# Driver](https://www.mongodb.com/docs/drivers/csharp/current/)
- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent.Kafka .NET client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
