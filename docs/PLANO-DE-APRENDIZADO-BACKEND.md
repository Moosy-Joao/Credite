# Plano de aprendizado — Back-End C#/.NET

**Objetivo:** construir competência prática para atuar como Desenvolvedor Back-End .NET e explicar decisões técnicas em entrevistas.

## Como estudar

Para cada assunto, seguir a sequência: conceito → problema que resolve → implementação → testes → revisão → explicação em entrevista. Não considerar um assunto dominado apenas por assistir a aulas ou copiar exemplos.

## 1. C# e .NET

- Tipos, classes, records, interfaces, generics e coleções.
- LINQ, nullable reference types e tratamento de exceções.
- async/await, Task, CancellationToken e concorrência básica.
- Injeção de dependência, configuração, logging e ciclo de vida.
- Organização de projetos, debugging e publicação.

**Evidência:** implementar casos de uso no Credite e explicar fluxo, dependências e tratamento de falhas.

## 2. ASP.NET Core e APIs REST

- Pipeline HTTP, middleware, Controllers e model binding.
- DTOs, validação, status codes e Problem Details.
- Autenticação, autorização, cookies e políticas.
- Paginação, filtros, ordenação e versionamento.
- OpenAPI e Scalar.
- Testes HTTP com WebApplicationFactory.

**Evidência:** API do Credite documentada, protegida e coberta por testes relevantes.

## 3. SQL Server e EF Core

- Modelagem relacional, chaves, constraints e relacionamentos.
- LINQ, migrations e transações.
- Índices, planos de execução e consultas.
- Concorrência, isolamento e integridade.
- Testes de integração com SQL Server descartável.

**Evidência:** fluxos do Credite persistidos com migrations e testes de integração.

## 4. Arquitetura e design

- SOLID aplicado a problemas reais.
- Clean Architecture e regra de dependências.
- DDD: entidades, value objects, agregados e invariantes.
- Monólito modular e limites entre módulos.
- Monólito versus microsserviços: trade-offs, custos e sinais para extração.

**Evidência:** justificar a arquitetura do Credite e explicar por que não extrair serviços prematuramente.

## 5. Testes e qualidade

- xUnit, Arrange-Act-Assert e testes parametrizados.
- Testes unitários versus integração.
- Fakes, mocks e testes de contrato.
- CI com restore, build e test.
- Code review, debugging e análise de regressões.

**Evidência:** pipeline reproduzível e suíte de testes útil.

## 6. PostgreSQL — laboratório comparativo

- Tipos, constraints, índices e transações.
- Níveis de isolamento e concorrência.
- EXPLAIN/ANALYZE e análise de consultas.
- Provider EF Core e diferenças em relação ao SQL Server.

**Evidência:** reproduzir um caso relacional em laboratório e documentar diferenças. SQL Server continua sendo o banco oficial do Credite.

## 7. MongoDB — laboratório documental

- Documentos, coleções e BSON.
- Modelagem orientada a consultas.
- Embedding versus referencing.
- Índices, filtros e atualizações atômicas.
- Consistência e transações.

**Evidência:** laboratório pequeno com consultas, testes e justificativa do modelo documental.

## 8. Apache Kafka — laboratório de mensageria

- Broker, topic, partition e offset.
- Producer, consumer e consumer group.
- Chaves, ordenação por partição e retenção.
- Semânticas de entrega e duplicidade.
- Idempotência, retry e dead-letter topic.
- Evolução e compatibilidade de eventos.
- Transactional Outbox como padrão avançado.

**Evidência:** producer/consumer em .NET com testes e demonstração de falha, duplicidade e reprocessamento seguro.

## 9. Microsserviços — laboratório posterior

- Limites de contexto e ownership de dados.
- Comunicação HTTP versus eventos.
- Falhas parciais, timeout e resiliência.
- Consistência eventual, Outbox e Saga.
- Observabilidade distribuída e correlação de traces.
- Custos de deploy, configuração, segurança e operação.

**Evidência:** laboratório de extração de um contexto pequeno, com contrato, dados próprios, testes e trade-offs documentados.

## Critério de domínio

Um tópico só será considerado dominado quando for possível:

1. Explicar o conceito com palavras próprias.
2. Implementar uma versão funcional sem copiar uma solução completa.
3. Escrever testes relevantes.
4. Diagnosticar uma falha.
5. Explicar limitações e alternativas.
6. Justificar quando não utilizaria a tecnologia.

## Como descrever no currículo

Diferenciar sempre:
- **Implementado no Credite:** tecnologias efetivamente usadas no produto.
- **Praticado em laboratório:** tecnologias com implementação e testes em projetos isolados.
- **Estudado:** conteúdo teórico ainda sem prática suficiente.

Não declarar experiência prática com PostgreSQL, MongoDB ou Kafka antes de concluir e validar os respectivos laboratórios.
