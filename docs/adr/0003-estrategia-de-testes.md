> **Nota de revisão (2026-11-07):** esta ADR foi escrita para testes de persistência com SQL Server no monólito. Com ADR-0014, atualizar a estratégia para PostgreSQL descartável nos serviços Identity, Customer e Credit, MongoDB descartável no Notification e testes de integração Kafka no fluxo Credit → Notification. Preservar os princípios gerais de testes unitários/integração e revisar módulos, containers e CI durante a reestruturação.

# ADR-0003 — Estratégia de testes automatizados

- **Estado:** Aceita
- **Data:** 2026-11-01
- **Responsáveis:** Projeto Credite

## Contexto

O Credite utiliza .NET 10, ASP.NET Core, EF Core 10 e Microsoft SQL Server. A solução começa como um monólito modular com projetos separados para domínio, aplicação, infraestrutura, API e interface MVC. A autenticação será baseada em ASP.NET Core Identity e cookies compartilhados.

A estratégia de testes precisa verificar regras de negócio isoladamente e também comprovar que os componentes de infraestrutura funcionam juntos, especialmente EF Core com SQL Server, o pipeline HTTP e os mecanismos de autenticação/autorização. Deve permanecer simples o suficiente para um projeto individual e evoluir conforme os fluxos forem implementados.

## Decisão

Adotar dois projetos de testes no escopo inicial:

- `Credite.UnitTests`: testes unitários de domínio e aplicação.
- `Credite.IntegrationTests`: testes de integração da API, persistência e autenticação/autorização.

### Testes unitários

- Manter xUnit como framework.
- Testar entidades, value objects, invariantes, validações, transições de estado e regras/cálculos demonstrativos.
- Testar casos de uso da Application isolando dependências externas por meio de abstrações/fakes quando apropriado.
- Não exigir banco de dados, Docker ou rede para executar esses testes.
- Priorizar testes determinísticos, rápidos e independentes entre si.

### Testes de integração

- Utilizar SQL Server real, preferencialmente provisionado de forma descartável com Testcontainers for .NET e Docker.
- Utilizar `Microsoft.AspNetCore.Mvc.Testing` e `WebApplicationFactory` para inicializar a API em um host de teste e enviar requisições HTTP.
- Validar um conjunto focado de operações de persistência (criação, leitura, atualização e exclusão quando aplicável), relacionamentos, constraints, transações, consultas relevantes e migrations.
- Validar endpoints, códigos HTTP, validações, respostas de erro e cenários importantes de autenticação/autorização com Identity e cookies.
- Manter os dados isolados por teste/classe/fixture, sem dependência da ordem de execução.
- Aplicar migrations no banco descartável antes dos cenários que dependem do schema e verificar que um banco vazio pode ser preparado.
- Não conectar testes a bancos de desenvolvimento, homologação ou produção.

### Organização

Estrutura planejada:

```text
tests/
├── Credite.UnitTests/
│   ├── Domain/
│   └── Application/
└── Credite.IntegrationTests/
    ├── Api/
    ├── Infrastructure/
    ├── Authentication/
    └── Fixtures/
```

A organização interna poderá acompanhar os módulos reais à medida que forem definidos. Não criar projetos de teste adicionais por camada sem necessidade demonstrada.

### Ferramentas

- xUnit para execução e organização dos testes.
- `Microsoft.NET.Test.Sdk` e `xunit.runner.visualstudio` para integração com o SDK e descoberta no Visual Studio/CLI.
- `Microsoft.AspNetCore.Mvc.Testing` para testes HTTP com `WebApplicationFactory`.
- Testcontainers for .NET (módulo SQL Server) e Docker para provisionar SQL Server descartável nos testes de integração.
- Coverlet poderá ser utilizado para coletar cobertura, sem impor um percentual global arbitrário nesta fase.

As versões dos pacotes deverão ser estáveis e compatíveis com .NET 10/EF Core 10 no momento da implementação, evitando referências a versões preview.

### Estratégia de cobertura e CI

- Cobrir primeiro comportamentos e riscos: invariantes do domínio, cálculos, transições, validações, persistência, migrations, contratos HTTP e autorização.
- Usar cobertura como indicador para localizar código sem testes, não como substituto da análise de qualidade nem como meta percentual sem justificativa.
- O GitHub Actions deverá executar restore, build, testes unitários e testes de integração.
- A etapa de integração deverá dispor de Docker e iniciar o SQL Server de testes. Falhas em qualquer etapa deverão reprovar o pipeline.
- Executar o conjunto automatizado em Pull Requests e em atualizações da branch principal, após validar a viabilidade no CI existente.

### Testes ponta a ponta

Adiar testes E2E com navegador (por exemplo, Playwright para .NET) até que as jornadas MVC estejam estáveis e exista benefício claro em validar o fluxo completo pela interface.

## Alternativas consideradas

- **Somente testes unitários:** execução rápida, mas não verifica integração real com SQL Server, EF Core, pipeline HTTP ou configuração de autenticação. Insuficiente isoladamente.
- **SQLite em memória como substituto do SQL Server:** pode ser útil em cenários específicos, mas possui diferenças de comportamento e tradução de consultas. Não será o provider principal dos testes de persistência, pois o banco oficial do projeto é SQL Server.
- **EF Core InMemory:** não reproduz o comportamento relacional do SQL Server e não será usado para validar consultas ou persistência.
- **SQL Server instalado e mantido manualmente:** tecnicamente válido, porém aumenta o esforço de configuração e dificulta isolamento/reprodutibilidade em máquinas e CI. Testcontainers é preferido para provisionamento descartável.
- **Testes E2E desde o início:** aumentariam custo e manutenção antes de as jornadas estarem estabilizadas; ficam adiados.

## Consequências

### Positivas
- Regras de negócio podem ser verificadas rapidamente sem infraestrutura externa.
- Persistência e consultas são validadas contra o mesmo mecanismo de banco escolhido para a aplicação.
- Os testes HTTP verificam a composição real da aplicação, incluindo middleware e configuração.
- A separação dos projetos permite executar unitários e integração de forma independente.
- O provisionamento descartável favorece repetibilidade local e no CI.

### Negativas / trade-offs
- Testes de integração são mais lentos e dependem de Docker.
- Testcontainers adiciona dependências de teste e exige configuração no pipeline.
- Fixtures, inicialização de containers, limpeza de dados e isolamento exigem disciplina.
- A cobertura total de jornadas de usuário ficará limitada até a adoção de E2E.

### Riscos e ações de mitigação
- **Container indisponível no CI:** validar Docker no workflow e documentar pré-requisitos locais.
- **Testes concorrentes interferindo nos dados:** usar bancos ou schemas isolados e controlar paralelismo quando necessário.
- **Migrations divergentes do modelo:** testar a criação do banco a partir de um estado vazio e revisar migrations no Git.
- **Testes excessivamente acoplados à implementação:** priorizar comportamento observável e contratos, evitando testar detalhes internos sem valor.
- **Segredos ou dados reais em testes:** utilizar somente dados fictícios e configuração de teste sem credenciais reais.

## Referências

- [Microsoft Learn — Integration tests in ASP.NET Core (.NET 10)](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
- [Microsoft Learn — Testing EF Core applications](https://learn.microsoft.com/en-us/ef/core/testing/)
- [Microsoft Learn — Choosing a testing strategy](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy)
- [Testcontainers for .NET — SQL Server module](https://dotnet.testcontainers.org/modules/mssql/)
- [Testcontainers for .NET — ASP.NET Core integration](https://dotnet.testcontainers.org/examples/aspnet/)
- [Docker — Testing an ASP.NET Core web app with Testcontainers](https://docs.docker.com/guides/testcontainers-dotnet-aspnet-core/)
- [xUnit — Getting started](https://xunit.net/docs/getting-started/v3/getting-started)
