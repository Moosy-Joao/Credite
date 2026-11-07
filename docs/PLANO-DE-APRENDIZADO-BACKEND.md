# Plano de aprendizado — Back-End C#/.NET

**Objetivo:** desenvolver competência prática para atuar como Desenvolvedor Back-End .NET e explicar decisões técnicas em entrevistas.

## Método

Para cada assunto: conceito → problema que resolve → implementação → testes → revisão → explicação em entrevista. Não considerar domínio apenas por assistir a aulas ou copiar exemplos.

## 1. C# 14 e .NET 10

Tipos, classes, records, interfaces, generics, LINQ, nullable reference types, exceções, async/await, Task, CancellationToken, concorrência, DI, configuração, logging, debugging e publicação.

**Evidência:** implementar e explicar casos de uso nos serviços Credite.

## 2. ASP.NET Core e APIs REST

Pipeline HTTP, middleware, Controllers, model binding, DTOs, validação, status codes, Problem Details, autenticação/autorização após ADR-0015, paginação, versionamento, OpenAPI, Scalar e WebApplicationFactory.

**Evidência:** APIs documentadas e testadas.

## 3. PostgreSQL, EF Core 10 e Npgsql

Modelagem relacional, constraints, índices, LINQ, migrations, transações, concorrência, isolamento e ownership de dados.

**Evidência:** Identity, Customer e Credit com bancos, DbContexts e migrations independentes.

## 4. MongoDB

Documentos, coleções, BSON, modelagem por consulta, embedding/referencing, índices, atualizações atômicas, consistência e deduplicação.

**Evidência:** Notification persistindo registros de processamento no MongoDB.

## 5. Apache Kafka

Broker, topics, partitions, offsets, producers, consumers, consumer groups, chaves, ordenação, retenção, semânticas de entrega, duplicidade, idempotência, retry, dead-letter topic, compatibilidade de eventos e Transactional Outbox.

**Evidência:** fluxo Credit → Kafka → Notification testado com falhas e reprocessamento.

## 6. Arquitetura

SOLID, DDD, agregados, invariantes, bounded contexts, ownership de dados, microsserviços versus monólito modular, HTTP versus eventos, consistência eventual, falhas parciais e resiliência.

**Evidência:** justificar os limites de Identity, Customer, Credit e Notification e os custos da distribuição.

## 7. Testes e qualidade

xUnit, Arrange-Act-Assert, testes unitários, integração e contrato, Testcontainers para PostgreSQL/MongoDB/Kafka, CI, debugging e code review.

**Evidência:** pipeline reproduzível e testes por serviço e fluxo integrado.

## 8. Segurança e observabilidade

Segredos, configuração, autenticação conforme ADR-0015, autorização, logs estruturados, OpenTelemetry, tracing distribuído, health checks e diagnóstico.

**Evidência:** rastrear uma operação entre Credit, Kafka e Notification sem expor dados sensíveis.

## Critério de domínio

Conseguir explicar, implementar sem copiar solução completa, testar, diagnosticar falha, explicar limitações e justificar quando não utilizar a tecnologia.

No currículo, diferenciar o que foi implementado no Credite, praticado em laboratório e apenas estudado. Não declarar experiência de produção para infraestrutura educacional.
