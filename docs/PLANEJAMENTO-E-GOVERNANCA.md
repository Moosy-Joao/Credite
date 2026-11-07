# Planejamento e governança do Credite

## Estado atual

A validação local inicial foi concluída em 2026-11-01: clone, abertura de Credite.slnx no Visual Studio Community, SDK .NET 10 e restore/build/test com sucesso. Não solicitar novamente essa validação como bloqueio.

Em 2026-11-07 foi aprovada a arquitetura de quatro microsserviços: Identity, Customer, Credit e Notification. Identity, Customer e Credit terão PostgreSQL independente; Notification terá MongoDB exclusivo. Kafka conecta Credit a Notification, com Outbox em Credit, idempotência, retry limitado e dead-letter topic em Notification.

A documentação foi consolidada. O código existente ainda precisa ser auditado e adaptado. Não presumir que a arquitetura já esteja implementada.

## Decisões aprovadas

- Quatro serviços: Identity, Customer, Credit e Notification.
- PostgreSQL por serviço para Identity, Customer e Credit.
- MongoDB exclusivo para Notification.
- EF Core 10 + Npgsql nos serviços PostgreSQL.
- Kafka no fluxo Credit → Notification.
- Outbox transacional em Credit.
- Consumidor idempotente, retry limitado e dead-letter topic em Notification.
- Projetos, namespaces e pastas arquiteturais em inglês; identificadores C# em português.

## Decisões pendentes — não presumir

- Protocolo de autenticação/autorização e emissão/validação de credenciais.
- Comunicação síncrona entre serviços.
- Contratos REST e políticas de acesso.
- Integração da interface administrativa: BFF, chamadas diretas, gateway ou outra composição.
- Necessidade e responsabilidade de API Gateway.
- Contratos, envelope, tópicos, chaves e versionamento dos eventos.
- Topologia de implantação e gestão de segredos fora do ambiente local.

Esses pontos pertencem a ADR-0015, cuja elaboração depende de confirmação explícita do responsável. Não criar ADR-0015 nem codificar decisões pendentes sem essa confirmação.

## Fluxo de trabalho

1. Planejamento.
2. Em especificação.
3. Decisões pendentes.
4. Pronto para execução.
5. Em andamento.
6. Validação e revisão.
7. Concluído.

## Definition of Ready

Objetivo, escopo, exclusões, dependências, riscos, critérios verificáveis, contratos e decisões críticas aprovadas.

## Definition of Done

Critérios de aceite atendidos; build e testes executados; isolamento de persistência comprovado; logs sem dados sensíveis; documentação atualizada; diff revisado; evidências reais registradas.

## Regras para agentes de código

- Começar por auditoria completa da solução, projetos, arquivos, dependências, migrations, testes e workflows.
- Classificar cada item como manter, adaptar, mover, substituir, remover ou aguardar ADR-0015.
- Não apagar/recriar arquivos antes de mapear finalidade e dependências.
- Preservar histórico de ADRs e migrations; propor baseline nova por serviço somente com justificativa.
- Não fazer substituição global cega de SQL Server por PostgreSQL.
- Não inventar contratos ou mecanismos pendentes de ADR-0015.
- Antes de operações destrutivas, apresentar arquivos afetados e justificativa.
- Não fazer commit, push, merge ou alterar branches sem autorização explícita.
- Não declarar build/teste aprovado sem executar e apresentar evidências.

## Estado documental

ADR-0014 é normativo para os limites dos quatro serviços, ownership dos bancos e fluxo Credit → Notification. ADR-0001, ADR-0012 e ADR-0013 permanecem no histórico como decisões substituídas nos pontos indicados. ADR-0015 depende de confirmação explícita.
