# Contrato de eventos

## Tópico: solicitacoes.credito.criada

Publicado pelo Credit API e consumido pelo Notification Worker.

### Payload

~~~json
{
  "eventoId": "d0c5d1f1-b85d-42c1-a913-94b77a0b10d8",
  "solicitacaoId": "c8bda67f-ccad-4f79-bb82-89dd86b4aef1",
  "clienteId": "9eeb04c1-b5e7-4a73-a2ea-ff6f72e7e3c2",
  "valorSolicitado": 2500.00,
  "criadoEm": "2026-11-13T15:00:00+00:00"
}
~~~

| Propriedade | Tipo | Descrição |
|---|---|---|
| eventoId | UUID | Identificador único usado para idempotência |
| solicitacaoId | UUID | Identificador da solicitação criada |
| clienteId | UUID | Identificador do cliente |
| valorSolicitado | decimal | Valor fictício solicitado |
| criadoEm | DateTimeOffset | Data e hora em UTC |

A chave Kafka da mensagem é o eventoId.

## Mensagens inválidas

O tópico solicitacoes.credito.criada.dlq recebe mensagens cujo JSON ou identificadores obrigatórios são inválidos. Falhas temporárias de persistência não são enviadas à DLQ: o Worker mantém a mensagem atual sem confirmar o offset e tenta novamente.

## Compatibilidade

O contrato é duplicado nos dois projetos para manter os microsserviços independentes de uma biblioteca compartilhada. Alterações devem atualizar os dois tipos e este documento no mesmo pull request.
