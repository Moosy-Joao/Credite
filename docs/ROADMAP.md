# Roadmap do Credite

## Estado
- Fase 0 — validação local: concluída em 2026-11-01.
- Arquitetura de quatro microsserviços: aprovada em 2026-11-07.
- Documentação arquitetural: consolidada.
- Auditoria e reestruturação do código: próxima etapa.
- ADR-0015: aguarda confirmação explícita.

## Fase 0 — validação local (concluída)

Clone, abertura de Credite.slnx no Visual Studio Community, SDK .NET 10 e restore/build/test foram confirmados com sucesso. Não repetir essa etapa.

## Fase 1 — auditoria e reestruturação

1. Inventariar projetos, namespaces, arquivos, dependências, endpoints, migrations, testes, Docker e workflows.
2. Classificar o legado: manter, adaptar, mover, substituir, remover ou aguardar ADR-0015.
3. Organizar projetos em inglês para Identity, Customer, Credit e Notification.
4. Separar PostgreSQL, DbContext e migrations para Identity, Customer e Credit.
5. Configurar MongoDB exclusivo do Notification.
6. Implementar Outbox em Credit e Kafka Credit → Notification com consumidor idempotente.
7. Atualizar testes, CI, configuração local e documentação.
8. Executar restore, build e testes após cada etapa.

Não implementar autenticação distribuída, comunicação síncrona, Gateway ou integração final da interface antes de ADR-0015. Não apagar histórico sem plano aprovado.

## Fase 2 — contratos e segurança (após ADR-0015)

Definir protocolo de autenticação/autorização, comunicação síncrona, contratos REST, integração da interface, eventual Gateway e contratos/eventos completos.

## Fase 3 — implementação funcional dos serviços

- Identity: contas internas, credenciais e perfis.
- Customer: clientes fictícios.
- Credit: solicitações, estados, avaliações, simulação e histórico.
- Notification: consumo de eventos e notificações demonstrativas.
- Persistência isolada, testes, segurança, health checks e observabilidade.

## Fase 4 — integração assíncrona

Validar Outbox, publicação Kafka, consumo idempotente, retry limitado, dead-letter topic, duplicidade, indisponibilidade do broker, reinício do publicador e redrive.

## Fase 5 — interface e fluxo vertical

Integrar a interface conforme ADR-0015 e demonstrar o fluxo de cliente, solicitação, avaliação demonstrativa e notificação.

## Fase 6 — qualidade e portfólio

Testes unitários, integração e contrato; CI; Docker Compose; observabilidade; documentação operacional; revisão de segurança e privacidade.

## Critérios de conclusão

Quatro serviços implementados; persistência exclusiva por serviço; fluxo Credit → Kafka → Notification resiliente; interface integrada conforme decisão aprovada; testes e CI passando; documentação fiel ao código; dados fictícios e limitações educacionais explícitos.
