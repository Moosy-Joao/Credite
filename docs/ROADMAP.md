# Roadmap do Credite

O projeto será concluído em incrementos verificáveis. O objetivo é demonstrar dois microsserviços independentes, persistência própria e comunicação assíncrona confiável.

## Etapa 1 — Estrutura mínima

- [x] Manter somente Credit API e Notification Worker.
- [x] Reduzir a solução para um projeto por serviço.
- [x] Simplificar documentação e decisões arquiteturais.
- [x] Configurar integração contínua de build.

## Etapa 2 — Credit API

- [x] Cadastrar e consultar clientes fictícios.
- [x] Criar e consultar solicitações de crédito.
- [x] Validar e normalizar dados de entrada.
- [x] Persistir dados no PostgreSQL usando EF Core.
- [x] Criar migration inicial.
- [x] Documentar a API com OpenAPI e Scalar.
- [x] Implementar testes unitários das regras essenciais.

## Etapa 3 — Mensageria e confiabilidade de publicação

- [x] Subir PostgreSQL e Kafka localmente com Docker Compose.
- [x] Definir o contrato do evento SolicitaçãoCriada.
- [x] Gravar solicitação e evento na mesma transação usando Outbox.
- [x] Publicar eventos pendentes no Kafka.
- [x] Limitar tentativas de publicação e registrar o último erro.

## Etapa 4 — Notification Worker

- [x] Consumir eventos do Kafka.
- [x] Persistir notificações em banco próprio.
- [x] Aplicar idempotência pelo identificador do evento.
- [x] Confirmar o offset somente após persistência.
- [x] Encaminhar mensagens inválidas para dead-letter topic.

## Etapa 5 — Validação e finalização

- [x] Configurar build e testes no GitHub Actions.
- [x] Validar snapshots das migrations automaticamente.
- [x] Documentar o contrato de eventos.
- [x] Documentar execução local e exemplos HTTP.
- [x] Validar o fluxo completo entre os serviços com PostgreSQL e Kafka no GitHub Actions.
- [x] Tratar conflito concorrente no cadastro de clientes.\n- [x] Paginar a listagem de solicitações com padrão 20 e máximo 100.\n- [x] Coordenar publicadores da Outbox com bloqueio transacional.\n- [x] Tratar duplicidade concorrente no consumidor.\n- [x] Enriquecer mensagens encaminhadas à DLQ com metadados.\n- [x] Verificar prontidão de PostgreSQL e Kafka na API.\n- [x] Permitir sobrescrita da conexão por variável de ambiente.\n- [ ] Executar o fluxo completo no ambiente local com Visual Studio Community.
- [ ] Revisar implementação no Visual Studio Community.

## Fora do escopo

Autenticação, interface Web, API Gateway, Redis, MongoDB, terceiro microsserviço, Saga e observabilidade distribuída avançada não fazem parte deste MVP.

## Limitações conhecidas

- Kafka e PostgreSQL são executados localmente por Docker Compose; os processos .NET são iniciados pelo desenvolvedor.
- A Outbox usa bloqueio transacional por registro e tenta publicar até dez vezes. Eventos que esgotarem as tentativas permanecem registrados para inspeção manual.
- O Worker aplica retry contínuo para falhas temporárias de persistência e não confirma o offset até concluir o processamento.
- A DLQ trata mensagens malformadas e preserva metadados de origem. Não existe ferramenta de replay automático no MVP.
- Os dados são fictícios e as regras de crédito são demonstrativas, sem análise financeira real.
