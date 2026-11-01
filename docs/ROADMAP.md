# Roadmap do Credite

Este roadmap organiza a descoberta e o planejamento. Não representa autorização para começar a implementar. O projeto permanece em planejamento até que as especificações sejam aprovadas e a verificação local inicial esteja concluída.

## Fase 0 — Preparação e verificação local

**Bloqueada até confirmação do responsável.**

- [ ] Clonar o repositório.
- [ ] Abrir `Credite.slnx` no Visual Studio Community.
- [ ] Verificar SDK .NET 10 instalado.
- [ ] Executar restore.
- [ ] Executar build.
- [ ] Executar testes.
- [ ] Registrar logs/resultados e corrigir problemas de estrutura.
- [ ] Confirmar explicitamente a conclusão.

Nenhuma implementação de produto deve começar antes desta fase.

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

- [ ] Validar monólito modular inicial.
- [ ] Definir dependências permitidas entre camadas.
- [ ] Definir contratos REST e convenções HTTP.
- [ ] Definir persistência e estratégia de migrations.
- [ ] Avaliar PostgreSQL versus SQL Server e registrar ADR.
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
- [ ] Definir comunicação HTTP via API/Gateway.
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

## Fase 6 — Arquitetura distribuída (somente quando justificada)

- [ ] Identificar limites de contexto e motivos reais para extração.
- [ ] Avaliar YARP como gateway.
- [ ] Avaliar RabbitMQ e MassTransit.
- [ ] Definir contratos e versionamento de eventos.
- [ ] Definir retries, DLQ e idempotência.
- [ ] Avaliar Transactional Outbox.
- [ ] Avaliar Saga e consistência eventual.
- [ ] Definir observabilidade distribuída.

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
