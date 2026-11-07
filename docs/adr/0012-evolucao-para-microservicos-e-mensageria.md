# ADR-0012 — Evolução para microserviços e mensageria

- Estado: substituído por ADR-0014 em 2026-11-07.
- Data original: 2026-10-31.
- Natureza: registro histórico.

## Decisão original

O Credite manteria monólito modular no MVP, com Customer e Credit como módulos internos, SQL Server compartilhado e sem microsserviços ou broker. Mensageria seria avaliada apenas diante de necessidade futura.

## Mudança aprovada

ADR-0014 substituiu a decisão de arquitetura-alvo. O Credite passa a ter quatro microsserviços:
- Identity — PostgreSQL próprio.
- Customer — PostgreSQL próprio.
- Credit — PostgreSQL próprio.
- Notification — MongoDB exclusivo.

Apache Kafka conecta Credit a Notification. Credit utiliza Transactional Outbox; Notification consome com idempotência, retry limitado e dead-letter topic.

A decisão anterior de evitar microsserviços no MVP não é mais normativa. Seus registros sobre custos de distribuição, contratos, falhas parciais, consistência eventual, observabilidade e operação continuam úteis como histórico.

## Referências

- [Microsoft — Microservices architecture style](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/microservices)
- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- ADR-0014.
