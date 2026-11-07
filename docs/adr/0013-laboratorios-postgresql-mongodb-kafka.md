# ADR-0013 — Laboratórios de PostgreSQL, MongoDB e Apache Kafka

- Estado: parcialmente substituído por ADR-0014 em 2026-11-07.
- Data original: 2026-11-07.
- Natureza: registro histórico.

## Decisão original

SQL Server seria o banco oficial do Credite; PostgreSQL, MongoDB e Apache Kafka seriam usados apenas em laboratórios isolados.

## Situação atual

ADR-0014 alterou essa separação:
- PostgreSQL passa a integrar Identity, Customer e Credit, com banco independente por serviço.
- MongoDB passa a ser utilizado exclusivamente por Notification.
- Apache Kafka passa a integrar o fluxo Credit → Notification.

Os laboratórios isolados deixam de ser a orientação principal para essas tecnologias no Credite. O conteúdo educacional e comparativo deste ADR permanece como histórico, mas não define a arquitetura-alvo.

## Referência normativa

ADR-0014 — Microsserviços, persistência e mensageria.
