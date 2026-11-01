# ADR-0001 — SQL Server e estratégia de persistência

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Responsável pelo projeto

## Contexto

O Credite precisa definir um padrão de banco de dados e uma estratégia de persistência que sejam adequados à solução .NET 10, ao uso de Entity Framework Core e ao objetivo educacional do projeto. A decisão deve manter o desenvolvimento local simples, migrations rastreáveis e evolução segura do esquema.

## Decisão

- **Banco de dados:** Microsoft SQL Server.
- **ORM:** Entity Framework Core 10, utilizando versões estáveis e compatíveis dos pacotes.
- **Abordagem:** Code First; o modelo do EF Core será a fonte de evolução do esquema.
- **Localização do contexto e migrations:** projeto `Credite.Infrastructure`, junto ao `DbContext`.
- **Versionamento:** migrations e snapshots serão commitados no Git.
- **Desenvolvimento local:** migrations serão criadas e aplicadas explicitamente por ferramentas do EF Core (CLI `dotnet ef` ou Package Manager Console do Visual Studio).
- **Implantação:** preferir scripts SQL revisáveis ou migration bundles, conforme o processo de publicação definido futuramente.
- **Inicialização:** a aplicação não aplicará migrations automaticamente durante a inicialização da API.

Esta decisão define a estratégia, mas não autoriza implementar ou configurar o banco antes da liberação formal da fase de construção.

## Alternativas consideradas

- **PostgreSQL:** alternativa tecnicamente válida e suportada pelo EF Core; não escolhida porque o projeto adotou SQL Server como padrão.
- **Database First:** não escolhido porque o projeto ainda está definindo o domínio e não possui um esquema legado que precise ser a fonte da verdade.
- **SQL manual como estratégia principal:** não escolhido porque Code First e migrations oferecem integração mais direta com o modelo C# e o ciclo de desenvolvimento previsto.
- **Projeto separado para migrations:** não adotado inicialmente; a estrutura atual não exige múltiplos conjuntos de migrations nem implantação independente.

## Consequências

### Positivas
- Integração direta com o ecossistema .NET e EF Core.
- Histórico de alterações do esquema versionado junto ao código.
- Desenvolvimento local reproduzível e adequado a testes.
- Separação clara entre domínio/aplicação e detalhes de persistência.

### Negativas / trade-offs
- O projeto fica acoplado ao SQL Server e ao provedor correspondente do EF Core.
- Migrations precisam ser revisadas e aplicadas de forma coordenada com as versões da aplicação.
- Scripts e bundles de implantação exigem processo explícito de revisão e execução.

### Riscos e ações de mitigação
- Não aplicar migrations automaticamente em produção.
- Revisar migrations e scripts antes de qualquer implantação.
- Manter os pacotes EF Core e o provedor SQL Server em versões compatíveis.
- Usar dados fictícios; não armazenar dados pessoais ou financeiros reais.

## Referências

- [EF Core — SQL Server provider](https://learn.microsoft.com/ef/core/providers/sql-server/)
- [EF Core — Migrations overview](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [EF Core — Applying migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying)
- [EF Core — Separate migrations project](https://learn.microsoft.com/ef/core/managing-schemas/migrations/projects)
