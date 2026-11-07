# Arquitetura do Credite

- Estado: arquitetura-alvo aprovada em 2026-11-07.
- Decisão normativa: ADR-0014.
- Pendências: ADR-0015, mediante confirmação explícita.
- Este documento não afirma que os serviços já estejam implementados ou implantados.

## 1. Visão geral

O Credite será composto por quatro microsserviços: Identity, Customer, Credit e Notification. Cada serviço terá responsabilidade delimitada, processo próprio e ownership exclusivo de persistência.

Identity, Customer e Credit utilizarão PostgreSQL, cada um com banco próprio. Notification utilizará exclusivamente MongoDB. Apache Kafka realizará o fluxo assíncrono aprovado entre Credit e Notification.

A interface administrativa .NET permanece como cliente da plataforma, não como quinto microsserviço de negócio. Não acessa bancos diretamente.

## 2. Limites e ownership

| Serviço | Responsabilidade | Dados próprios | Persistência |
|---|---|---|---|
| Identity | Contas internas, credenciais, perfis e identidade | Usuários, credenciais, roles | PostgreSQL |
| Customer | Cadastro e manutenção de clientes fictícios | Dados cadastrais e regras de cliente | PostgreSQL |
| Credit | Solicitações, estados, avaliações demonstrativas, simulações e histórico | Solicitações, avaliações, histórico e Outbox | PostgreSQL |
| Notification | Consumo de eventos e notificações demonstrativas | Registros de notificação e deduplicação | MongoDB |

Regras obrigatórias:
- Um serviço é a única autoridade de leitura/escrita sobre seus dados.
- Nenhum serviço consulta ou altera diretamente o banco de outro.
- Não compartilhar tabelas, schemas, DbContexts, migrations, entidades de persistência ou transações.
- Referências entre serviços utilizam identificadores e contratos.
- Transações ACID são locais ao banco de cada serviço.
- Cada serviço evolui seu schema/documentos independentemente.
- Identity, Customer e Credit usam EF Core 10 + Npgsql em versões estáveis compatíveis com .NET 10.
- Notification usa o driver oficial MongoDB .NET em versão estável compatível.

## 3. Fluxo aprovado Credit → Kafka → Notification

1. Credit altera o estado de negócio.
2. Na mesma transação PostgreSQL, grava o evento pendente na Outbox.
3. Um publicador recuperável envia eventos pendentes ao Kafka.
4. Após confirmação de publicação, atualiza o estado da Outbox.
5. Notification consome e persiste o resultado no MongoDB.
6. O consumidor é idempotente, pois mensagens podem ser entregues mais de uma vez.
7. Falhas transitórias recebem retry limitado; mensagens não processadas são encaminhadas para dead-letter topic.

A garantia prática é entrega pelo menos uma vez, Outbox recuperável e idempotência. Não prometer exactly-once ponta a ponta. Nome, versão, envelope, payload, tópicos, partições, chaves e retenção dos eventos devem ser definidos antes de implementar os contratos.

## 4. Comunicação ainda não decidida

A comunicação síncrona entre Identity, Customer e Credit não foi definida. Não presumir chamadas HTTP, eventos adicionais, BFF, API Gateway, YARP ou acesso direto da interface aos serviços. Esses pontos dependem de ADR-0015 e confirmação explícita.

## 5. Persistência

### PostgreSQL — Identity, Customer e Credit
- Banco independente por serviço.
- DbContext, connection string, configurações e migrations independentes.
- EF Core 10 com Npgsql.
- Migrations versionadas, explícitas e revisáveis; não aplicar automaticamente em produção.
- Tabelas e colunas em português com snake_case.
- Sem banco ou schema compartilhado.

### MongoDB — Notification
- Banco e coleções exclusivos do serviço.
- Modelagem orientada às consultas e ao ciclo de vida das notificações.
- Identificador estável de evento/mensagem e mecanismo persistente de deduplicação.
- Não replicar dados de Customer/Credit além do mínimo necessário ao evento.

## 6. Segurança e identidade

Identity é dono dos dados de identidade. O protocolo de autenticação, emissão/validação de credenciais, sessão da interface e autorização entre serviços ainda não foi aprovado.

Não presumir compartilhamento de cookies do monólito anterior nem introduzir JWT, OAuth 2.0, OpenID Connect, API Gateway ou outro mecanismo sem ADR-0015. Cada serviço deverá validar autorização no seu limite de confiança conforme a decisão futura.

## 7. Interface administrativa

A interface .NET existente permanece cliente da plataforma. Não possui persistência própria nem regras de domínio. A forma de integração com Identity, Customer e Credit (BFF, chamadas diretas, gateway ou outra composição) depende de ADR-0015.

## 8. Observabilidade e testes

Cada serviço deve ter health checks, logs estruturados e correlação. OpenTelemetry deve cobrir chamadas HTTP e mensagens Kafka quando implementadas. Nunca registrar senhas, tokens, cookies, connection strings ou dados pessoais.

Testes:
- unitários por serviço;
- integração com PostgreSQL descartável para Identity, Customer e Credit;
- integração com MongoDB descartável para Notification;
- fluxo Kafka Credit → Notification;
- Outbox, duplicidade, idempotência, retry, dead-letter e recuperação;
- contratos REST/eventos após sua aprovação.

## 9. Convenções de nomenclatura

Projetos, namespaces e pastas arquiteturais em inglês. Identificadores C# em português, seguindo PascalCase/camelCase, prefixo I para interfaces e sufixo Async para métodos assíncronos. Banco em português snake_case. Testes e documentação em português. Tipos oficiais do .NET e termos técnicos permanecem em inglês.

## 10. Diagrama de arquitetura aprovada

~~~mermaid
flowchart LR
    Usuario["Usuário interno"] --> Web["Interface administrativa .NET"]
    Web -. "integração pendente ADR-0015" .-> Identity["Identity"]
    Web -. "integração pendente ADR-0015" .-> Customer["Customer"]
    Web -. "integração pendente ADR-0015" .-> Credit["Credit"]
    Identity --> PGI[("PostgreSQL Identity")]
    Customer --> PGC[("PostgreSQL Customer")]
    Credit --> PGL[("PostgreSQL Credit + Outbox")]
    Credit -->|"Outbox"| Kafka[("Apache Kafka")]
    Kafka -->|"consumo idempotente"| Notification["Notification"]
    Notification --> Mongo[("MongoDB Notification")]
~~~

O diagrama não define protocolo de autenticação, chamadas síncronas, gateway ou topologia de produção.

## 11. Histórico de decisões

- ADR-0001 (SQL Server) substituído por ADR-0014.
- ADR-0005 (arquitetura de implantação monolítica) substituído nos pontos conflitantes.
- ADR-0012 (monólito modular como arquitetura-alvo) substituído.
- ADR-0013 (PostgreSQL, MongoDB e Kafka apenas como laboratórios) parcialmente substituído.
- ADR-0002, ADR-0010 e ADR-0011 precisam ser revisados quanto às premissas de autenticação, interface e operação distribuída. Preservar histórico.
- ADR-0015 tratará integração da interface, autenticação/autorização, comunicação síncrona e eventual Gateway, somente após confirmação.

## Referências

- [Microsoft — Microservices architecture style](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/microservices)
- [Microsoft — Data sovereignty per microservice](https://learn.microsoft.com/dotnet/architecture/microservices/architect-microservice-container-applications/data-sovereignty-per-microservice)
- [Npgsql — EF Core Provider](https://www.npgsql.org/efcore/)
- [MongoDB — .NET/C# Driver](https://www.mongodb.com/docs/drivers/csharp/current/)
- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [Confluent.Kafka .NET client](https://docs.confluent.io/kafka-clients/dotnet/current/overview.html)
