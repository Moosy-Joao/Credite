# Roadmap do Credite

O objetivo é concluir um fluxo funcional entre dois microsserviços, avançando em incrementos pequenos. Uma etapa só começa quando a anterior estiver compilando e validada.

## Etapa 1 — Estrutura mínima

- [x] Manter somente Credit API e Notification Worker.
- [x] Reduzir a solução para um projeto por serviço.
- [x] Simplificar a documentação e as decisões arquiteturais.
- [ ] Restaurar e compilar localmente no Visual Studio Community.

## Etapa 2 — Credit API

- [ ] Criar o modelo simples de Cliente.
- [ ] Criar o modelo de Solicitação de Crédito.
- [ ] Implementar cadastro e consulta de clientes fictícios.
- [ ] Implementar criação e consulta de solicitações.
- [ ] Persistir dados no PostgreSQL usando EF Core.
- [ ] Criar testes unitários e de integração essenciais.

## Etapa 3 — Mensageria

- [ ] Subir PostgreSQL e Kafka localmente com Docker Compose.
- [ ] Definir o contrato do evento SolicitaçãoCriada.
- [ ] Publicar o evento pelo Credit após registrar uma solicitação.
- [ ] Verificar publicação e consumo com uma mensagem real.

## Etapa 4 — Notification Worker

- [ ] Consumir eventos do Kafka.
- [ ] Processar e registrar notificações fictícias.
- [ ] Persistir notificações em banco próprio.
- [ ] Tratar mensagens inválidas e falhas de processamento.

## Etapa 5 — Confiabilidade

- [ ] Implementar idempotência.
- [ ] Implementar Transactional Outbox.
- [ ] Implementar retry limitado.
- [ ] Implementar dead-letter topic.
- [ ] Testar duplicidade, indisponibilidade e recuperação.

## Etapa 6 — Finalização

- [ ] Completar testes automatizados relevantes.
- [ ] Documentar como executar toda a solução.
- [ ] Validar build, testes e fluxo completo localmente.
- [ ] Revisar README e registrar limitações conhecidas.

## Regra de escopo

Não adicionar autenticação, interface Web, API Gateway, Redis, MongoDB, terceiro microsserviço ou projetos separados por camada durante o MVP. Qualquer mudança de escopo exige justificativa e decisão explícita.
