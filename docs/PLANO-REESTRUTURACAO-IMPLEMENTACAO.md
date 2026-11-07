# Plano de reestruturação do código existente

- Estado: auditoria e preparação técnica autorizadas.
- Base: ADR-0014.
- Branch documental: docs/foco-backend-csharp-net.
- Limite: não implementar decisões pendentes de ADR-0015.

## 1. Contexto

O código atual foi estruturado sob decisões anteriores de monólito modular, SQL Server, Identity e hosts MVC/API. A arquitetura aprovada em 2026-11-07 possui quatro serviços: Identity, Customer, Credit e Notification. Os três primeiros usam PostgreSQL independente; Notification usa MongoDB exclusivo; Kafka conecta Credit a Notification com Outbox, idempotência, retry limitado e dead-letter topic.

A tarefa não é apenas adicionar quatro projetos ao monólito antigo. É auditar, mapear e reestruturar o legado para que não permaneçam dois caminhos concorrentes.

## 2. Inventário obrigatório antes de editar

Inventariar solução e projetos; namespaces; endpoints; middleware; entidades e DTOs; DbContexts e migrations; providers e connection strings; Identity, cookies e Data Protection; testes; Docker/Compose; workflows; scripts e documentação.

Classificar cada item como manter, adaptar, mover, substituir, remover ou aguardar ADR-0015.

## 3. Mudanças aprovadas

- Organizar projetos e namespaces em inglês para Identity, Customer, Credit e Notification.
- Manter a interface administrativa como cliente .NET separado, sem acesso a banco nem regras de domínio.
- Identity, Customer e Credit: PostgreSQL independente, DbContext e migrations independentes.
- Notification: MongoDB exclusivamente.
- Credit: Outbox na mesma transação PostgreSQL da alteração de negócio.
- Kafka: publicação a partir da Outbox e consumo idempotente no Notification.
- Retry limitado para falhas transitórias e dead-letter topic após esgotamento.
- Identificadores internos C# em português; projetos, namespaces e pastas em inglês.
- Atualizar testes, configuração local, documentação e CI conforme a arquitetura real.

## 4. Tratamento do legado

Avaliar referências a SQL Server/SqlClient/provider SQL Server, DbContexts compartilhados, migrations antigas, tabelas Identity, entidades cruzadas, lógica em API/Web, cookie compartilhado, projetos Credite.Api/Application/Domain/Infrastructure/Web/Contracts, testes e Docker/workflows.

Não substituir SQL Server por PostgreSQL por busca e substituição global. Não apagar migrations antigas sem avaliar seu valor histórico e definir estratégia de baseline independente por serviço. Não manter caminhos de produção duplicados após a migração.

## 5. Bloqueios de ADR-0015

Não decidir nem implementar por conta própria:
- JWT, OAuth 2.0, OpenID Connect, cookies compartilhados ou outro protocolo;
- sessão da interface;
- comunicação síncrona entre serviços;
- BFF, API Gateway, YARP ou roteamento central;
- contratos REST e políticas distribuídas de autorização;
- payloads e versionamento definitivos de eventos além do fluxo genérico aprovado.

Se um item for pré-requisito, parar nesse ponto, explicar a dependência e apresentar opções com trade-offs. Não criar arquitetura implícita temporária.

## 6. Sequência

1. Auditoria sem alterações.
2. Mapeamento dos projetos atuais para serviços e interface.
3. Relatório de arquivos a mover, adaptar, substituir ou remover.
4. Reestruturação da solução.
5. Separação de domínio e aplicação por serviço.
6. Persistência PostgreSQL por serviço e MongoDB em Notification.
7. Kafka, Outbox e consumidor idempotente.
8. Testes unitários e integração.
9. Docker Compose, CI, health checks e observabilidade.
10. Relatório final com arquivos e decisões bloqueadas.

## 7. Aceite

Sem acesso cruzado a bancos; PostgreSQL e migrations independentes nos três serviços; MongoDB exclusivo no Notification; Outbox transacional; Kafka com idempotência, retry limitado e dead-letter topic; interface sem acesso direto a bancos; testes executados e reportados; nenhuma decisão de ADR-0015 presumida; documentação fiel ao código.

## 8. Transições técnicas registradas

### 8.1 SQL Server no Identity (Etapa 2 para Etapa 3)

- Estado: transição técnica autorizada em 2026-11-07, durante a Etapa 2. Não é uma decisão arquitetural: o ADR-0014 continua sendo a referência.
- Escopo: Credite.Identity.Infrastructure (pacote Microsoft.EntityFrameworkCore.SqlServer, UseSqlServer e a migration InitialIdentity) e Credite.Identity.IntegrationTests (Testcontainers.MsSql).
- Condição: **obrigatoriamente encerrada na Etapa 3**, com PostgreSQL, baseline nova de migration, mapeamento explícito do Identity para português snake_case e remoção dos pacotes e da fixture de SQL Server.
- Controle: a lista ProjetosComExcecaoDeSqlServer, em RegrasDeArquiteturaTestes, e os comentários "TRANSIÇÃO TÉCNICA" em Directory.Packages.props. Ao fim da Etapa 3 a lista deve estar vazia e as exceções removidas.

### 8.2 Nomes legados em inglês (Etapa 2 para Etapa 3)

Foram movidos sem renomeação de identificadores: ApplicationUser, CrediteDbContext, AddInfrastructure e as classes e métodos dos testes de integração do Identity. A chave de configuração CrediteDb e o UserSecretsId do host do Identity também foram mantidos, para preservar o segredo local durante a transição. Tudo isso será revisto na Etapa 3, quando esses arquivos forem reescritos.
