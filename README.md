# Credite

Plataforma educacional de gestão de solicitações de crédito fictícias, desenvolvida com C# 14 e .NET 10. Não realiza análise financeira real, não concede crédito e não utiliza dados pessoais ou financeiros reais.

## Arquitetura aprovada

A arquitetura-alvo possui quatro microsserviços:

| Serviço | Responsabilidade | Persistência |
|---|---|---|
| Identity | Contas internas, credenciais, perfis e identidade | PostgreSQL próprio |
| Customer | Cadastro e manutenção de clientes fictícios | PostgreSQL próprio |
| Credit | Solicitações, simulações demonstrativas, avaliações e histórico | PostgreSQL próprio |
| Notification | Consumo de eventos e processamento de notificações demonstrativas | MongoDB exclusivo |

Cada serviço é dono exclusivo de seus dados. Não há banco, schema, DbContext, migrations ou transação compartilhados entre serviços.

Apache Kafka conecta Credit a Notification. Credit grava a alteração de negócio e o evento pendente na mesma transação PostgreSQL por meio de Transactional Outbox. Notification consome os eventos com idempotência, retry limitado e dead-letter topic. Não se promete exactly-once ponta a ponta.

A interface administrativa .NET permanece como cliente da plataforma, não como quinto microsserviço de negócio, e não acessa bancos diretamente. A integração da interface, autenticação distribuída, comunicação síncrona entre serviços e eventual API Gateway dependem de ADR-0015, ainda não aprovado.

## Tecnologias

| Área | Tecnologia |
|---|---|
| Linguagem/plataforma | C# 14 / .NET 10 |
| APIs | ASP.NET Core Web API |
| Bancos relacionais | PostgreSQL por serviço |
| ORM | EF Core 10 + Npgsql |
| Banco documental | MongoDB exclusivo do Notification |
| Mensageria | Apache Kafka |
| Cliente Kafka .NET | Confluent.Kafka, versão estável compatível |
| Interface | ASP.NET Core MVC / Razor |
| Testes | xUnit e testes de integração com bancos descartáveis |
| Observabilidade | ILogger, OpenTelemetry e Health Checks |
| Ambiente local | Docker / Docker Compose |

## Convenções de nomenclatura

- Solução, projetos, namespaces e pastas arquiteturais: inglês.
- Identificadores internos C#: português, respeitando as convenções oficiais.
- Classes, métodos e propriedades: PascalCase (Emprestimo, CriarEmprestimoAsync, ValorTotal).
- Interfaces: I + PascalCase (IRepositorioEmprestimo).
- Parâmetros e variáveis: camelCase (idCliente, valorEmprestimo).
- Campos privados: _camelCase.
- Métodos assíncronos: sufixo Async.
- Tabelas e colunas: português em snake_case (emprestimos, data_contratacao).
- Testes: português e nomes descritivos (DeveCriarEmprestimoComDadosValidos).
- Tipos e APIs oficiais permanecem em inglês (DbContext, CancellationToken, Task).
- Documentação e mensagens de commit: português.

## Escopo

Os perfis internos previstos são Administrador, Analista e Operador. O fluxo funcional abrange clientes fictícios, solicitações, simulações e avaliações demonstrativas e histórico. Notification faz parte da arquitetura, mas canais externos reais não estão automaticamente autorizados.

Consulte [requisitos funcionais](docs/REQUISITOS-MVP.md), [arquitetura](docs/ARQUITETURA.md) e [plano de reestruturação](docs/PLANO-REESTRUTURACAO-IMPLEMENTACAO.md).

## Estado do repositório

A validação local inicial foi concluída em 2026-11-01: clone, abertura de Credite.slnx no Visual Studio Community, SDK .NET 10, restore, build e testes com sucesso. Não repetir essa etapa como bloqueio.

A branch documental é docs/foco-backend-csharp-net. O código existente foi criado sob decisões anteriores de monólito modular e SQL Server e precisa ser auditado e adaptado. Esta documentação não afirma que os quatro serviços já estejam implementados.

## Documentação

- [Arquitetura](docs/ARQUITETURA.md)
- [Plano de reestruturação](docs/PLANO-REESTRUTURACAO-IMPLEMENTACAO.md)
- [Requisitos funcionais](docs/REQUISITOS-MVP.md)
- [Roadmap](docs/ROADMAP.md)
- [Governança](docs/PLANEJAMENTO-E-GOVERNANCA.md)
- [Plano de aprendizado](docs/PLANO-DE-APRENDIZADO-BACKEND.md)
- [Primeiro incremento — especificação anterior a revisar](docs/PRIMEIRO-INCREMENTO.md)
- [ADR-0001 — SQL Server, histórico substituído](docs/adr/0001-sql-server-e-estrategia-de-persistencia.md)
- [ADR-0012 — monólito modular, histórico substituído](docs/adr/0012-evolucao-para-microservicos-e-mensageria.md)
- [ADR-0013 — laboratórios, histórico parcialmente substituído](docs/adr/0013-laboratorios-postgresql-mongodb-kafka.md)
- [ADR-0014 — arquitetura atual aprovada](docs/adr/0014-microservicos-persistencia-e-mensageria.md)

ADR-0015 depende de confirmação explícita. Não presumir suas decisões.

## Validação da solução existente

Os comandos abaixo validam a solução atualmente versionada; não comprovam por si só a arquitetura-alvo:

~~~bash
dotnet restore Credite.slnx
dotnet build Credite.slnx
dotnet test Credite.slnx
~~~

A topologia final e os comandos de execução serão atualizados após a reestruturação e a definição das pendências de ADR-0015.

## Aviso

O Credite é educacional. Não é uma instituição financeira e não deve ser utilizado para decisões financeiras reais.
