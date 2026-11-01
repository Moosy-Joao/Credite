# Credite

Plataforma demonstrativa de gestão de solicitações de crédito, desenvolvida com C# e .NET. O projeto tem finalidade educacional e de portfólio: utiliza dados fictícios e regras simuladas, sem realizar análise financeira real.

## Objetivos

- Desenvolver uma aplicação Back-End com ASP.NET Core e APIs REST.
- Aplicar modelagem de domínio, validação, persistência e testes automatizados.
- Construir uma interface administrativa com ASP.NET Core MVC e Razor Views.
- Evoluir gradualmente de um monólito modular para serviços independentes.
- Demonstrar comunicação assíncrona, resiliência, observabilidade e consistência eventual.

## Escopo funcional

- Cadastro e consulta de clientes fictícios.
- Registro e acompanhamento de solicitações de crédito.
- Simulação demonstrativa de parcelas e condições.
- Avaliação por regras de negócio configuráveis e fictícias.
- Histórico de alterações e notificações simuladas.

## Arquitetura e evolução

O desenvolvimento começa com uma aplicação modular, mantendo limites explícitos entre domínio, aplicação, infraestrutura e apresentação. A separação em microserviços ocorrerá somente quando houver uma necessidade arquitetural demonstrável.

Arquitetura-alvo, a ser introduzida por etapas:

- **API Gateway:** YARP como ponto de entrada para as APIs.
- **Customer:** cadastro e consulta de clientes fictícios.
- **Credit:** solicitações e simulações.
- **Risk Analysis:** execução de regras demonstrativas de avaliação.
- **Notification:** notificações simuladas.

A interface MVC consome as APIs pelo Gateway. Ela não acessa bancos de dados nem executa regras pertencentes a outros serviços.

## Tecnologias previstas

| Área | Tecnologia |
|---|---|
| Linguagem e plataforma | C# 14 / .NET 10 |
| API | ASP.NET Core Web API |
| Interface | ASP.NET Core MVC, Razor Views e Bootstrap |
| Persistência | Entity Framework Core |
| Banco de dados | Microsoft SQL Server (decisão registrada no ADR-0001) |
| Identidade | ASP.NET Core Identity com cookies compartilhados (ADR-0002) |
| Mensageria | RabbitMQ e MassTransit |
| Gateway | YARP |
| Cache | Redis, quando houver necessidade |
| Testes | xUnit |
| Contêineres | Docker e Docker Compose |
| Observabilidade | OpenTelemetry, logs estruturados e Health Checks |

As dependências serão adicionadas conforme a necessidade de cada etapa. Recursos nativos do .NET serão priorizados quando forem suficientes.

## Plano incremental

1. Definir requisitos, domínio e decisões arquiteturais.
2. Criar a solução .NET, projetos, configuração e testes.
3. Implementar o fluxo inicial de clientes e solicitações no monólito modular.
4. Adicionar persistência, migrations, validações e testes de integração.
5. Criar a interface MVC consumindo a API.
6. Preparar Docker Compose e observabilidade.
7. Extrair serviços gradualmente e introduzir Gateway e mensageria.
8. Implementar padrões distribuídos somente quando o fluxo justificar: Outbox, idempotência, retries, Circuit Breaker, DLQ e Saga.

## Planejamento e governança

O projeto está em fase de planejamento. A implementação permanece bloqueada até a confirmação da clonagem do repositório e da primeira validação local (restore, build e testes).

- [Governança, regras e Definition of Done](docs/PLANEJAMENTO-E-GOVERNANCA.md)
- [Roadmap e fases do projeto](docs/ROADMAP.md)
- [Automações GitHub recomendadas para .NET](docs/AUTOMACOES-GITHUB-DOTNET.md)
- [Modelo de ADR](docs/adr/0000-template.md)
- [ADR-0001 — SQL Server e estratégia de persistência](docs/adr/0001-sql-server-e-estrategia-de-persistencia.md)
- [ADR-0002 — Autenticação, autorização e dados fictícios](docs/adr/0002-autenticacao-autorizacao-e-dados-ficticios.md)
- [ADR-0003 — Estratégia de testes automatizados](docs/adr/0003-estrategia-de-testes.md)
- [ADR-0004 — Contratos REST, erros e convenções HTTP](docs/adr/0004-contratos-rest-erros-e-convencoes-http.md)
- [ADR-0005 — Arquitetura inicial e limites entre camadas](docs/adr/0005-arquitetura-inicial-e-limites-entre-camadas.md)
- [ADR-0006 — Requisitos não funcionais e critérios de qualidade](docs/adr/0006-requisitos-nao-funcionais-e-criterios-de-qualidade.md)

## Estrutura inicial

```text
Credite/
├── src/
│   ├── Credite.Api/
│   ├── Credite.Web/
│   ├── Credite.Application/
│   ├── Credite.Domain/
│   └── Credite.Infrastructure/
├── tests/
│   └── Credite.UnitTests/
└── Credite.slnx
```

A solução contém uma API, uma aplicação MVC e os projetos de domínio, aplicação e infraestrutura. A interface MVC será cliente HTTP da API; os projetos de domínio e aplicação não dependem da camada de apresentação.

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

A API expõe o endpoint inicial de verificação em `/health`.

Para iniciar a interface MVC, em outro terminal:

```bash
dotnet run --project src/Credite.Web
```

As URLs locais serão exibidas pelo próprio comando `dotnet run`.

## Qualidade

- Testes automatizados para regras de domínio e fluxos de aplicação.
- Contratos HTTP explícitos e códigos de status adequados.
- Configurações sensíveis fornecidas por variáveis de ambiente ou User Secrets.
- Nenhuma credencial, chave ou dado pessoal real versionado.
- Commits pequenos e objetivos, escritos em português.

## Aviso

O Credite é um projeto educacional. Não é uma instituição financeira, não concede crédito e não deve ser utilizado para decisões financeiras reais.
