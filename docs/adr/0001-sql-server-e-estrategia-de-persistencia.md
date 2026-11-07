# ADR-0001 — SQL Server e estratégia de persistência

- Estado: substituído por ADR-0014 em 2026-11-07.
- Data original: 2026-10-31.
- Natureza: registro histórico; não normativo para a arquitetura atual.

## Decisão original

O Credite adotaria Microsoft SQL Server, EF Core 10, Code First, migrations versionadas no projeto Infrastructure e aplicação explícita das migrations. A aplicação não executaria migrations automaticamente ao iniciar.

## Situação atual

ADR-0014 substituiu essa decisão:
- Identity, Customer e Credit utilizam PostgreSQL, cada um com banco próprio.
- EF Core 10 + Npgsql é a estratégia relacional.
- Cada serviço possui DbContext e migrations independentes.
- Notification utiliza MongoDB exclusivamente.

SQL Server deixa de ser o banco oficial da arquitetura-alvo. Esta ADR é preservada para registrar a decisão anterior e não deve orientar novas implementações.

## Referências

- [EF Core — Migrations](https://learn.microsoft.com/ef/core/managing-schemas/migrations/)
- [Npgsql — EF Core Provider](https://www.npgsql.org/efcore/)
- ADR-0014 — Microsserviços, persistência e mensageria.
