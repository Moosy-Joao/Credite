# ADR-0005 — Arquitetura inicial e limites entre camadas

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Projeto Credite

## Contexto

O Credite precisa manter responsabilidades explícitas entre domínio, casos de uso, infraestrutura, API e interface administrativa. A solução começa como um monólito modular, com ASP.NET Core Web API e ASP.NET Core MVC em aplicações distintas. A interface MVC deve consumir a API por HTTP, sem acessar diretamente o banco de dados nem executar regras de negócio do backend.

A estrutura inicial já contém os projetos `Credite.Api`, `Credite.Web`, `Credite.Application`, `Credite.Domain` e `Credite.Infrastructure`. É necessário formalizar as responsabilidades e dependências permitidas e definir como os contratos HTTP serão compartilhados.

## Decisão

Adotar Clean Architecture com organização interna por funcionalidades, preservando inicialmente os projetos existentes e adicionando `Credite.Contracts`.

### Responsabilidades

- **Credite.Domain:** entidades, Value Objects, agregados, invariantes e regras essenciais do domínio. Não depende de frameworks de apresentação, persistência ou serviços externos.
- **Credite.Application:** casos de uso, orquestração e abstrações necessárias para executar operações. Depende do Domain e não conhece implementações concretas da Infrastructure.
- **Credite.Infrastructure:** implementações técnicas, incluindo EF Core, SQL Server, migrations, persistência, Identity e integrações externas. Implementa abstrações definidas pelas camadas internas.
- **Credite.Api:** camada de entrada HTTP do backend: controllers, autenticação, autorização, contratos HTTP, tradução entre contratos e casos de uso e composição das dependências.
- **Credite.Web:** aplicação MVC administrativa, com controllers MVC, Views, ViewModels, navegação e clientes HTTP. Consome a API exclusivamente por HTTP; não referencia Domain, Application ou Infrastructure e não acessa diretamente o banco de dados.
- **Credite.Contracts:** biblioteca compartilhada de contratos públicos HTTP, contendo DTOs de requisição e resposta organizados por funcionalidade. Não contém entidades de domínio, regras de negócio, ViewModels ou dependências de infraestrutura.

### Dependências permitidas entre projetos

- `Credite.Domain`: não referencia outros projetos da solução.
- `Credite.Application`: referencia `Credite.Domain`.
- `Credite.Infrastructure`: referencia `Credite.Application` e `Credite.Domain`.
- `Credite.Api`: referencia `Credite.Application`, `Credite.Infrastructure` e `Credite.Contracts`.
- `Credite.Web`: referencia `Credite.Contracts` e nenhum dos projetos internos de backend.
- `Credite.Contracts`: não referencia outros projetos da solução.

A referência da API para Infrastructure serve à composição das dependências no ponto de entrada. Controllers não devem acessar diretamente DbContext, repositórios ou detalhes de persistência. Os casos de uso devem ser acionados por abstrações da Application.

### Organização modular

As funcionalidades serão organizadas por áreas de negócio dentro dos projetos, por exemplo `Customers`, `CreditRequests` e `RiskAssessment`. Não serão criados projetos separados para cada módulo neste momento. Limites definitivos e interações entre módulos serão refinados durante a modelagem do domínio.

### Composição e comunicação

- A API é o ponto de composição das dependências do backend e registra Application e Infrastructure.
- A Web registra seus serviços de apresentação e clientes HTTP tipados para consumir a API.
- Os registros de dependências podem ser agrupados em métodos de extensão por camada, mantendo os pontos de entrada legíveis.
- A autenticação compartilhada entre MVC e API seguirá o ADR-0002: esquema e configuração de cookies compatíveis, chaves e nome de aplicação Data Protection compartilhados e autorização validada pela API em cada operação protegida.
- A API permanece responsável por Identity e persistência de contas. A Web não recebe acesso ao banco de identidade.

## Alternativas consideradas

- **Um único projeto:** reduz a quantidade inicial de projetos, mas enfraquece o isolamento das responsabilidades e não aproveita a separação já criada.
- **Camadas tradicionais com Application dependente de Infrastructure:** simplifica alguns acessos, mas inverte a direção desejada das dependências e dificulta testes isolados.
- **DTOs duplicados na API e na Web:** evita biblioteca compartilhada, porém cria duplicação e risco de divergência entre contratos.
- **Cliente gerado a partir de OpenAPI desde o primeiro incremento:** é uma opção válida, mas adiciona geração e configuração de código sem necessidade comprovada neste estágio.
- **Microserviços desde o início:** aumentam o custo operacional e de comunicação antes de existirem evidências de autonomia ou necessidade de implantação independente.

## Consequências

### Positivas

- Domínio independente de infraestrutura e apresentação.
- Fronteiras de dependência verificáveis por referências entre projetos.
- MVC isolado do banco e das regras de negócio do backend.
- Contratos HTTP explícitos e compartilhados entre API e MVC.
- Organização por capacidades de negócio sem multiplicação prematura de projetos.
- Caminho evolutivo para extração de serviços quando houver justificativa concreta.

### Negativas / trade-offs

- A biblioteca de contratos cria acoplamento de versão entre API e MVC, que evoluem coordenadamente no estágio inicial.
- Há mais projetos e mapeamentos do que em uma aplicação única e simples.
- A equipe precisa respeitar as dependências permitidas; a estrutura sozinha não impede todas as violações internas.

### Riscos e ações de mitigação

- **Contratos compartilhados acumularem regras ou dependências:** manter `Credite.Contracts` limitado a DTOs HTTP, sem lógica de domínio ou dependências de infraestrutura.
- **Controllers contornarem Application:** manter controllers finos e cobrir contratos e autorização com testes de integração.
- **Módulos acessarem detalhes uns dos outros:** definir interfaces e contratos explícitos conforme os limites do domínio forem descobertos.
- **Configuração incorreta de cookies compartilhados:** validar esquema, Data Protection e autorização em testes de integração antes de liberar o fluxo autenticado.

## Referências

- [Microsoft — Common web application architectures](https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
- [Microsoft — Infrastructure persistence layer design](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/infrastructure-persistence-layer-design)
- [Microsoft — Design a DDD-oriented microservice](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)
- [Microsoft — Share authentication cookies among ASP.NET apps](https://learn.microsoft.com/aspnet/core/security/cookie-sharing)
- ADR-0012 — Evolução para microserviços e mensageria.
- ADR-0001 — SQL Server e estratégia de persistência.
- ADR-0002 — Autenticação, autorização e dados fictícios.
- ADR-0003 — Estratégia de testes automatizados.
- ADR-0004 — Contratos REST, erros e convenções HTTP.
