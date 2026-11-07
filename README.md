# Credite

Plataforma demonstrativa de gestão de solicitações de crédito, desenvolvida com C# e .NET. O projeto tem finalidade educacional e de portfólio: utiliza dados fictícios e regras simuladas, sem realizar análise financeira real.

## Objetivos

- Desenvolver uma aplicação Back-End com ASP.NET Core e APIs REST.
- Aplicar modelagem de domínio, validação, persistência e testes automatizados.
- Construir uma interface administrativa com ASP.NET Core MVC e Razor Views.
- Evoluir gradualmente a arquitetura, mantendo o monólito modular no MVP e estudando serviços independentes em laboratório quando houver justificativa.
- Demonstrar boas práticas de engenharia, segurança, rastreabilidade e qualidade de software.

## Escopo funcional do MVP

O MVP entrega um fluxo vertical completo de gestão fictícia:

- Autenticação e administração de contas internas com os perfis Administrador, Analista e Operador.
- Cadastro, consulta e edição de clientes fictícios.
- Registro, consulta e acompanhamento de solicitações.
- Simulação e avaliação demonstrativa com regras explícitas e resultados explicáveis.
- Histórico das principais operações, com autoria e data/hora.

O escopo detalhado, os requisitos funcionais RF-001 a RF-026, a matriz de permissões, o fluxo de aceite e as exclusões estão em [Requisitos funcionais e escopo do MVP](docs/REQUISITOS-MVP.md).

O MVP não inclui análise ou decisão de crédito real, dados pessoais/financeiros reais, integrações financeiras, portal de clientes, notificações externas, mensageria, cache distribuído, API Gateway ou microserviços.

## Arquitetura e evolução

O desenvolvimento começa como monólito modular, mantendo limites explícitos entre domínio, aplicação, infraestrutura e apresentação. A separação em microserviços ocorrerá somente quando uma necessidade arquitetural demonstrável justificar seus custos operacionais e de complexidade.

Os limites funcionais começam como módulos internos, não como serviços implantados separadamente. No MVP, Customer cuida dos clientes fictícios e Credit cuida das solicitações, avaliações, histórico e simulação. A simulação não é um serviço de análise de risco independente; Notification ainda não é um contexto funcional aprovado. Os critérios obrigatórios para uma extração futura, incluindo ownership de dados, autonomia, evidências operacionais e plano de migração, estão no ADR-0012.

A interface administrativa usa MVC server-rendered com Razor Views e Bootstrap 5.3, consumindo a API por HTTP. Não acessa diretamente o banco nem executa regras de negócio. Jornadas, ViewModels, estados de tela, segurança e critérios de acessibilidade estão definidos no ADR-0010. No MVP, a comunicação não depende de um API Gateway.

## Tecnologias

| Área | Tecnologia |
|---|---|
| Linguagem e plataforma | C# 14 / .NET 10 |
| API | ASP.NET Core Web API |
| Interface | ASP.NET Core MVC, Razor Views e Bootstrap |
| Persistência | Entity Framework Core 10 |
| Banco de dados | Microsoft SQL Server (ADR-0001) |
| Identidade | ASP.NET Core Identity com cookies compartilhados (ADR-0002) |
| Testes | xUnit; testes de integração com SQL Server descartável (ADR-0003) |
| Contêineres | Docker e Docker Compose, conforme necessidade de desenvolvimento/testes |
| Observabilidade | `ILogger`, OpenTelemetry/OTLP, Health Checks e Aspire Dashboard standalone opcional (ADR-0011) |

Tecnologias de evolução futura, não obrigatórias para o MVP:

- Apache Kafka será a tecnologia principal do laboratório educacional de mensageria, sem integração automática ao MVP. RabbitMQ/MassTransit permanecem alternativas a comparar caso um fluxo real justifique broker.
- PostgreSQL será estudado em laboratório comparativo com SQL Server; SQL Server continua sendo o banco oficial do Credite.
- MongoDB será estudado em laboratório de modelagem documental, sem persistência paralela no MVP.
- YARP somente se múltiplos backends criarem uma necessidade real de gateway.
- Redis para cache, quando houver necessidade medida.
- Aspire Dashboard standalone opcional para inspeção local de logs, traces e métricas exportados por OTLP (ADR-0011).
- Outbox, idempotência, retry/redelivery, Circuit Breaker, DLQ e Saga somente quando um fluxo distribuído específico exigir essas garantias (ADR-0012).

As dependências serão adicionadas conforme a necessidade de cada etapa. Recursos nativos do .NET serão priorizados quando forem suficientes.

## Plano incremental

1. Definir requisitos, domínio e decisões arquiteturais.
2. Validar a solução localmente e preparar configuração e testes.
3. Implementar clientes fictícios e solicitações no monólito modular.
4. Implementar estados, histórico, simulações e avaliações demonstrativas.
5. Integrar a interface MVC consumindo a API.
6. Completar testes de integração, segurança, acessibilidade e critérios de aceite.
7. Preparar execução local com Docker e observabilidade conforme necessidade.
8. Avaliar extração de serviços, Gateway e mensageria somente com justificativa concreta.

## Planejamento e governança

A validação local inicial foi concluída em 2026-11-01: o repositório foi clonado, `Credite.slnx` foi aberto no Visual Studio Community, o SDK .NET 10 foi confirmado e restore, build e testes foram executados com sucesso. Essa etapa não é mais um bloqueio nem deve ser solicitada novamente. O desenvolvimento segue por incrementos pequenos, conforme o roadmap.

- [Governança, regras e Definition of Done](docs/PLANEJAMENTO-E-GOVERNANCA.md)
- [Roadmap e fases do projeto](docs/ROADMAP.md)
- [Requisitos funcionais e escopo do MVP](docs/REQUISITOS-MVP.md)
- [Especificação e critérios de aceite do primeiro incremento](docs/PRIMEIRO-INCREMENTO.md)
- [Automações GitHub recomendadas para .NET](docs/AUTOMACOES-GITHUB-DOTNET.md)
- [Visão arquitetural e diagramas C4](docs/ARQUITETURA.md)
- [Plano de aprendizado Back-End C#/.NET](docs/PLANO-DE-APRENDIZADO-BACKEND.md)
- [Modelo de ADR](docs/adr/0000-template.md)
- [ADR-0001 — SQL Server e estratégia de persistência](docs/adr/0001-sql-server-e-estrategia-de-persistencia.md)
- [ADR-0002 — Autenticação, autorização e dados fictícios](docs/adr/0002-autenticacao-autorizacao-e-dados-ficticios.md)
- [ADR-0003 — Estratégia de testes automatizados](docs/adr/0003-estrategia-de-testes.md)
- [ADR-0004 — Contratos REST, erros e convenções HTTP](docs/adr/0004-contratos-rest-erros-e-convencoes-http.md)
- [ADR-0005 — Arquitetura inicial e limites entre camadas](docs/adr/0005-arquitetura-inicial-e-limites-entre-camadas.md)
- [ADR-0006 — Requisitos não funcionais e critérios de qualidade](docs/adr/0006-requisitos-nao-funcionais-e-criterios-de-qualidade.md)
- [ADR-0007 — Visão do produto, atores e limites do Credite](docs/adr/0007-visao-do-produto-atores-e-limites.md)
- [ADR-0008 — Modelagem do domínio e ciclo de vida da solicitação](docs/adr/0008-modelagem-do-dominio-e-ciclo-de-vida-da-solicitacao.md)
- [ADR-0009 — Regras demonstrativas de simulação e avaliação](docs/adr/0009-regras-demonstrativas-de-simulacao-e-avaliacao.md)
- [ADR-0010 — Interface administrativa MVC e jornadas](docs/adr/0010-interface-administrativa-e-jornadas-mvc.md)
- [ADR-0011 — Observabilidade e operação local](docs/adr/0011-observabilidade-e-operacao-local.md)
- [ADR-0012 — Evolução para microserviços e mensageria](docs/adr/0012-evolucao-para-microservicos-e-mensageria.md)
- [ADR-0013 — Laboratórios de PostgreSQL, MongoDB e Apache Kafka](docs/adr/0013-laboratorios-postgresql-mongodb-kafka.md)

## Estrutura inicial

```text
Credite/
├── src/
│   ├── Credite.Api/
│   ├── Credite.Web/
│   ├── Credite.Application/
│   ├── Credite.Domain/
│   ├── Credite.Infrastructure/
│   └── Credite.Contracts/       # definido no ADR-0005
├── tests/
│   ├── Credite.UnitTests/
│   └── Credite.IntegrationTests/ # definido no ADR-0003
└── Credite.slnx
```

A estrutura representa os projetos previstos nas decisões aprovadas; a existência e compilação de todos eles ainda precisam ser confirmadas na validação local. A interface MVC será cliente HTTP da API; domínio e aplicação não dependem da camada de apresentação.

## Executar localmente

Requisito: .NET 10 SDK.

Na raiz do repositório:

```bash
dotnet restore Credite.slnx
dotnet build Credite.slnx
dotnet test Credite.slnx
```

Para iniciar a API:

```bash
dotnet run --project src/Credite.Api
```

A API expõe `/health/live` para liveness e `/health/ready` para readiness com verificação do SQL Server; `/health` permanece como alias da readiness. A aplicação MVC expõe `/health/live`. Os detalhes estão no ADR-0011.

Para iniciar a interface MVC, em outro terminal:

```bash
dotnet run --project src/Credite.Web
```

As URLs locais serão exibidas pelo próprio comando `dotnet run`.

## Qualidade

- Testes unitários para regras de domínio e fluxos de aplicação.
- Testes de integração com SQL Server descartável, conforme ADR-0003.
- Contratos HTTP explícitos e códigos de status adequados.
- Autorização aplicada no servidor e testada na API.
- Configurações sensíveis fornecidas por User Secrets ou variáveis de ambiente.
- Nenhuma credencial, chave ou dado pessoal real versionado.
- Commits pequenos e objetivos, escritos em português.

## Aviso

O Credite é um projeto educacional. Não é uma instituição financeira, não concede crédito e não deve ser utilizado para decisões financeiras reais.
