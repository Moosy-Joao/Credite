# Roadmap do Credite

Este roadmap orienta a evolução do produto e a formação prática em Back-End C#/.NET. A validação local inicial foi concluída em 2026-11-01; não é mais pré-requisito pendente. O MVP continua como monólito modular em .NET 10, com SQL Server. Laboratórios de PostgreSQL, MongoDB, Kafka e microsserviços são trilhas isoladas e não ampliam automaticamente o escopo do produto.

## Fase 0 — Preparação e verificação local

**Status: concluída em 2026-11-01 pelo responsável.**

- [x] Clonar o repositório.
- [x] Abrir `Credite.slnx` no Visual Studio Community.
- [x] Verificar SDK .NET 10 instalado.
- [x] Executar restore.
- [x] Executar build.
- [x] Executar testes.
- [x] Registrar a conclusão.
- [x] Confirmar explicitamente a conclusão.

Não solicitar novamente essa validação como bloqueio. Reexecutar restore/build/test quando uma alteração técnica justificar, como parte normal do ciclo de desenvolvimento.

## Fase 1 — Visão e requisitos

- [ ] Definir personas e atores.
- [ ] Definir problema, objetivos e limites do produto.
- [ ] Definir casos de uso e jornadas.
- [ ] Definir requisitos funcionais e não funcionais.
- [ ] Definir vocabulário do domínio.
- [ ] Definir escopo do MVP e itens fora do escopo.
- [ ] Definir critérios de aceite por fluxo.

## Fase 2 — Domínio e regras

- [ ] Mapear entidades, value objects e agregados candidatos.
- [ ] Definir estados e transições de uma solicitação.
- [ ] Definir invariantes e regras de validação.
- [ ] Definir cálculo demonstrativo de parcelas e juros.
- [ ] Definir trilha de auditoria e histórico.
- [ ] Definir idempotência e duplicidade de solicitações.
- [ ] Documentar exemplos e casos-limite.

## Fase 3 — Arquitetura e decisões técnicas

- [x] Aprovar monólito modular inicial (ADR-0005 e ADR-0012).
- [ ] Definir dependências permitidas entre camadas.
- [ ] Definir contratos REST e convenções HTTP.
- [ ] Definir persistência e estratégia de migrations.
- [x] Definir SQL Server como banco oficial do Credite (ADR-0001).
- [ ] Criar laboratório comparativo de PostgreSQL, sem substituir o banco principal.
- [ ] Definir autenticação, autorização e gestão de segredos.
- [ ] Definir estratégia de testes.
- [ ] Definir logging, métricas, tracing e health checks.
- [ ] Definir estratégia de configuração e ambientes.
- [ ] Definir estratégia de versionamento e compatibilidade de API.

## Fase 4 — Experiência administrativa

- [ ] Mapear telas e navegação.
- [ ] Definir responsabilidades da aplicação MVC.
- [ ] Definir ViewModels e estados de interface.
- [ ] Definir acessibilidade e feedback de erros.
- [x] Definir comunicação HTTP entre MVC e API; Gateway não faz parte do MVP (ADR-0010 e ADR-0012).
- [ ] Definir limites entre apresentação e regras de negócio.

## Fase 5 — Operação e produtividade

- [ ] Revisar CI já existente e validar execução.
- [ ] Definir política de branches e pull requests.
- [ ] Definir templates de issue e pull request.
- [ ] Definir EditorConfig e regras de formatação.
- [ ] Avaliar Dependabot para NuGet e GitHub Actions.
- [ ] Avaliar CodeQL e secret scanning.
- [ ] Definir Docker Compose e ambientes locais.
- [ ] Definir observabilidade e diagnóstico.
- [ ] Definir política de releases e changelog.

## Fase 6 — Laboratório de Apache Kafka

**Trilha educacional isolada; não integra o MVP.**

- [ ] Entender broker, topics, partitions, offsets e consumer groups.
- [ ] Implementar producer e consumer em .NET.
- [ ] Definir contratos de eventos versionados e chaves de particionamento.
- [ ] Demonstrar processamento idempotente e tratamento de duplicidade.
- [ ] Implementar política limitada de retry e dead-letter topic.
- [ ] Exercitar reprocessamento, falhas de consumidor e evolução de contratos.
- [ ] Estudar Transactional Outbox depois de dominar transações e publicação de eventos.
- [ ] Documentar garantias de entrega e limitações; não prometer exactly-once sem comprovação.

## Fase 7 — Laboratórios de bancos de dados

**Laboratórios isolados; SQL Server continua sendo o banco do Credite.**

### PostgreSQL
- [ ] Reproduzir um caso relacional do Credite em laboratório.
- [ ] Comparar tipos, constraints, índices, transações e concorrência com SQL Server.
- [ ] Exercitar consultas e análise de planos de execução.
- [ ] Comparar o comportamento dos providers do EF Core.

### MongoDB
- [ ] Modelar um caso de uso documental independente.
- [ ] Comparar embedding e referencing.
- [ ] Exercitar filtros, índices, atualizações atômicas e validação de documentos.
- [ ] Documentar quando o modelo documental é adequado ou inadequado para o domínio.

## Fase 8 — Arquitetura distribuída e microsserviços

**Somente após domínio, API, persistência, testes e laboratório Kafka.**

- [ ] Identificar limites de contexto estáveis e capacidades de negócio autônomas.
- [ ] Selecionar um cenário pequeno para extração demonstrativa.
- [ ] Definir ownership de dados e contratos HTTP/eventos.
- [ ] Implementar tratamento de timeout, falhas parciais, retry e idempotência.
- [ ] Demonstrar consistência eventual; estudar Outbox e Saga quando aplicáveis.
- [ ] Instrumentar logs, métricas e tracing distribuído.
- [ ] Comparar custo operacional com o monólito modular.
- [ ] Registrar a extração em ADR próprio; não decompor por entidade ou por modismo.

## Critério para liberar construção

A construção só começa quando:
1. Fases de requisitos, domínio e arquitetura tiverem especificações aprovadas para o primeiro incremento.
2. Decisões essenciais estiverem registradas.
3. Critérios de aceite e testes do primeiro incremento estiverem definidos.
4. A Fase 0 estiver confirmada como concluída pelo responsável.

## Escopo inicial candidato

A confirmar durante a especificação:
- cadastro de clientes fictícios;
- registro e consulta de solicitações;
- simulação demonstrativa de condições;
- avaliação por regras fictícias;
- histórico de estados e notificações simuladas.

O escopo final do MVP será registrado após análise dos requisitos, não presumido por este documento.
