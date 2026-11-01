# ADR-0012 — Evolução para microserviços e mensageria

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Projeto Credite
- **Decisões relacionadas:** ADR-0005 (arquitetura inicial), ADR-0007 (visão e limites do produto), ADR-0008 (modelo de domínio), ADR-0009 (simulação) e ADR-0011 (observabilidade)

## Contexto

O Credite foi concebido como monólito modular com API ASP.NET Core, interface MVC, SQL Server e limites internos explícitos. O modelo de domínio já distingue Customer e CreditRequest como raízes de agregados; isso protege invariantes, mas não significa, por si só, que cada agregado deva se tornar um processo, banco ou serviço independente.

O backlog também considera os contextos candidatos Risk Analysis e Notification, além de API Gateway, RabbitMQ, MassTransit, Outbox, idempotência, retry, Dead Letter Queue (DLQ) e Saga.

O projeto é desenvolvido inicialmente por uma pessoa, tem escopo de portfólio/educacional e não possui, no MVP, integrações financeiras, notificações externas, carga medida que exija escala independente ou transações de negócio entre serviços. A arquitetura distribuída criaria custos de implantação, operação, diagnóstico, testes e consistência sem resolver uma necessidade atual.

## Decisão

**Manter o Credite como monólito modular no MVP e não introduzir microserviços, broker ou API Gateway nesta fase.** Os limites de módulos e contratos internos devem ser preservados para permitir evolução futura, sem antecipar separação física.

### 1. Organização lógica do monólito

Adotar limites funcionais internos, sem criar um projeto/serviço por entidade:

- **Customer:** responsabilidade pelo cadastro e manutenção dos clientes fictícios.
- **Credit:** responsabilidade pelo ciclo de vida de CreditRequest, Assessment, histórico e fluxo de avaliação demonstrativa.
- **Simulação demonstrativa:** permanece uma capacidade interna do contexto Credit neste MVP. Não existe, ainda, um contexto autônomo de análise de risco financeiro.
- **Identity e autorização:** permanecem transversais e sob as decisões do ADR-0002; não são um serviço de negócio separado.
- **Notification:** não faz parte do MVP. Sua existência como contexto, eventos e regras só será especificada quando uma necessidade concreta de notificação for aprovada.

As fronteiras lógicas devem evitar acesso direto às tabelas e regras pertencentes a outro módulo. A comunicação entre módulos será feita por interfaces/casos de uso explícitos dentro do processo, sem acoplamento a classes internas ou entidades de persistência de outro módulo.

Customer e Credit podem compartilhar o banco SQL Server e transações locais durante o MVP. A separação de agregados do ADR-0008 continua válida e não implica criar bancos ou serviços separados.

Os projetos API e Web são hosts de apresentação/comunicação definidos no ADR-0005. Essa separação não deve ser confundida com extrair os contextos de negócio em microserviços adicionais.

### 2. Quando considerar extrair um contexto como microserviço

Uma extração futura só poderá ser proposta quando **todos** os seguintes critérios forem demonstrados:

1. **Capacidade de negócio autônoma:** o contexto atende a uma capacidade de negócio coesa, com vocabulário, regras e ciclo de vida próprios. Não basta uma entidade, tabela, camada técnica ou CRUD isolado.
2. **Benefício operacional concreto:** existe evidência de uma necessidade de deploy independente, escala independente, isolamento de falhas, requisitos operacionais distintos ou cadência de mudança que cause impacto real quando o contexto permanece no monólito. O benefício precisa superar o custo total da distribuição.
3. **Baixo acoplamento e contrato estável:** operações típicas não dependem de chamadas síncronas frequentes entre serviços. Relações muito comunicativas indicam que os componentes provavelmente devem permanecer juntos.
4. **Propriedade exclusiva dos dados:** o novo serviço é o único responsável por escrever e controlar seu modelo e esquema. Outros serviços não consultam nem atualizam suas tabelas diretamente. A integração acontece por contrato/API ou evento.
5. **Operação independente possível:** há pipeline de CI/CD, testes de contrato/integração, configuração e segredos, health checks, logs, métricas, tracing, alertas e procedimentos de falha/recuperação suficientes para operar e depurar o serviço.
6. **Migração reversível e justificada:** existe plano de extração incremental, compatibilidade de contratos, migração de dados, rollback e critérios de sucesso observáveis. Não realizar uma reescrita big-bang apenas para adotar o estilo.

A decisão de extração deverá registrar evidências e custos em ADR próprio. Não será definida por quantidade de projetos, número de entidades, conveniência de separar equipes no futuro ou preferência tecnológica.

### 3. Candidatos de decomposição — sem compromisso de extração

| Capacidade | Decisão atual | Condição para reavaliação |
|---|---|---|
| Customer | Módulo interno do monólito | Necessidade comprovada de ciclo de vida, escala ou deploy autônomo e ownership dos dados |
| Credit | Módulo interno do monólito | Necessidade comprovada de operação/deploy independente, sem chamadas excessivamente acopladas com Customer |
| Risk Analysis | Parte do módulo Credit; simulação demonstrativa | Tornar-se uma capacidade de negócio autônoma com regras, evolução, carga ou integrações próprias; a simples execução da fórmula Price não justifica serviço |
| Notification | Fora do MVP e ainda sem contexto funcional aprovado | Requisito real de entrega de notificações com canais, garantias, preferências, tentativas e ciclo de vida próprios |

Não há decisão de que todos esses candidatos serão extraídos. A decomposição deve refletir coesão de negócio observada, não um diagrama antecipado de serviços.

### 4. Comunicação síncrona e assíncrona

- Manter MVC → API via HTTP conforme ADR-0010 e os contratos REST do ADR-0004.
- Dentro do monólito, priorizar chamadas diretas a casos de uso/serviços de aplicação com limites explícitos.
- Não publicar mensagens em RabbitMQ para operações CRUD ou cálculos locais curtos que precisam devolver um resultado imediato ao usuário.
- Considerar mensageria quando existir trabalho realmente assíncrono, execução prolongada, necessidade de desacoplar produtor e consumidor em processos distintos, absorção de picos/backpressure ou integração eventual entre serviços autônomos.
- Não usar broker apenas para evitar uma chamada de método entre módulos do mesmo processo.
- Para um trabalho interno temporário e não durável, avaliar primeiro recursos do .NET como `BackgroundService` ou `System.Threading.Channels`. Eles não substituem uma fila durável: se a mensagem precisa sobreviver à queda do processo e ser consumida por outra instância/serviço, é necessária uma solução persistente apropriada.

Se a mensageria se tornar necessária, avaliar RabbitMQ como broker e MassTransit como abstração .NET para consumidores, roteamento e padrões de resiliência. A seleção de versões, topologia, filas, exchanges, transporte, persistência e estratégia de operação deve ocorrer em ADR específico próximo da implementação, com compatibilidade estável com .NET 10 verificada nessa data.

### 5. Garantias de entrega e padrões distribuídos — condicionais

Caso um fluxo futuro publique eventos a partir de uma transação SQL:

- **Transactional Outbox:** adotar quando a alteração de estado e a publicação do evento precisarem ser confiáveis como uma unidade lógica. Persistir o evento pendente na mesma transação do estado de negócio e publicar depois, com rastreamento de entrega. Não publicar diretamente no broker antes/depois do commit sem tratar a falha de dual-write.
- **Idempotência:** consumidores precisam tolerar mensagens duplicadas. Usar identificador estável da mensagem/evento e mecanismo de deduplicação/inbox ou operação naturalmente idempotente. Não prometer entrega exactly-once.
- **Acknowledgements e publisher confirms:** se RabbitMQ for escolhido, configurar confirmações de publicação e confirmação do consumidor somente após processamento/persistência adequada. O modelo usual de entrega é pelo menos uma vez, portanto duplicatas continuam possíveis.
- **Retry/redelivery:** limitar tentativas e aplicar somente a falhas transitórias. Preferir backoff exponencial com jitter e prazos máximos quando o cenário exigir. Erros de validação ou regras de negócio permanentes não devem ser repetidos. Escritas só podem ser repetidas quando forem idempotentes ou protegidas contra duplicação.
- **Dead Letter Queue (DLQ):** configurar quando houver broker e política de retries definida. Mensagens sem processamento após o limite devem ser isoladas, observadas e ter procedimento explícito de análise/correção/redrive; não fazer requeue infinito.
- **Circuit Breaker e timeout:** aplicar a chamadas remotas instáveis quando a falha/latência de uma dependência puder causar cascata. Não adicionar Circuit Breaker indiscriminadamente a chamadas internas.
- **Saga:** usar somente quando um processo de negócio longo atravessar múltiplas transações/serviços autônomos, com estado de coordenação, eventos e ações compensatórias. Não aplicar Saga a uma operação que pode ser concluída atomicamente dentro de um único serviço e banco.

Esses padrões resolvem problemas distintos e não são um pacote obrigatório. A escolha de cada um dependerá do fluxo, das falhas possíveis e da garantia de negócio necessária.

### 6. API Gateway e YARP

Não introduzir YARP no MVP. MVC já consome um único backend definido, a API; um gateway nesse cenário apenas adicionaria mais um salto, configuração e ponto de falha.

Reavaliar Gateway quando houver múltiplos serviços backend acessados por mais de um cliente e necessidade comprovada de roteamento central, composição/agregação, políticas de entrada compartilhadas, rate limiting ou gerenciamento de tráfego. O Gateway deve tratar preocupações transversais e encaminhamento, nunca regras de domínio.

Mesmo após uma extração, o uso de Gateway não é automático: clientes internos podem consumir APIs diretamente enquanto isso permanecer simples e seguro.

### 7. Plano de migração caso os critérios sejam atendidos

A extração deverá ocorrer uma capacidade por vez, em abordagem incremental (por exemplo, Strangler Fig), evitando reescrita completa:

1. Confirmar contexto delimitado e contrato de integração.
2. Tornar explícitos os limites do módulo ainda dentro do monólito.
3. Estabelecer ownership e plano de migração dos dados, sem acesso compartilhado de escrita.
4. Extrair o serviço e sua persistência, mantendo compatibilidade com o consumidor legado durante a transição.
5. Adotar eventos, Outbox e consistência eventual apenas nos fluxos que exigirem essas garantias.
6. Instrumentar tracing distribuído, health checks, métricas, logs e alarmes antes de colocar o serviço em produção.
7. Executar testes de contrato, integração, falha parcial e recuperação.
8. Remover o caminho legado somente após comprovar equivalência funcional, estabilidade e possibilidade de rollback.

A primeira capacidade a extrair não está pré-determinada. Ela será escolhida com base nos sinais e dependências reais, não a partir da ordem da tabela de candidatos.

## Alternativas consideradas

- **Microserviços desde o início:** rejeitado porque o MVP não tem necessidade de escala, ownership operacional ou deploy independente; acrescentaria consistência eventual, contratos entre processos, observabilidade distribuída e operações adicionais.
- **Um serviço por entidade/agregado:** rejeitado porque entidades e agregados são limites de consistência do domínio, não necessariamente limites de implantação. Separações excessivamente granulares criam chamadas e coordenação desnecessárias.
- **Monólito sem módulos internos:** rejeitado porque permitiria dependências e acesso a dados pouco controlados, dificultando futura evolução.
- **Monólito modular com extração orientada por evidências — escolhido:** permite aprender e entregar o fluxo ponta a ponta, mantendo coesão e contratos internos, e preserva opção de extração quando existir motivo real.
- **Mensageria em todas as operações:** rejeitado; operações síncronas e locais não precisam de broker, e mensagens introduzem duplicidade, ordenação, observabilidade e consistência eventual.
- **RabbitMQ/MassTransit, Outbox, retries e Saga imediatamente:** rejeitado por antecipar falhas e fluxos distribuídos ainda inexistentes.

## Consequências

### Positivas

- Menos infraestrutura e pontos de falha no MVP.
- Deploy, desenvolvimento local, depuração e testes mais simples para uma pessoa.
- A separação de agregados e módulos continua explícita sem confundir domínio com implantação.
- Critérios claros para justificar futuras mudanças arquiteturais.
- Mensageria e padrões de confiabilidade serão aplicados com base em cenários reais.

### Negativas / trade-offs

- O deploy das capacidades de negócio continua coordenado dentro da API/monólito.
- Não haverá escala independente por módulo.
- Se surgir uma necessidade real de separação, será necessário executar uma migração planejada e não apenas ligar um broker.

### Riscos e ações de mitigação

- **Monólito torna-se acoplado:** preservar limites internos, contratos de módulo e ownership das tabelas.
- **Extração prematura por modismo:** exigir evidências para cada critério obrigatório e registrar ADR novo.
- **Broker introduz duplicidade/perda percebida:** usar confirmação de publicação/consumo, Outbox quando houver dual-write e consumidores idempotentes.
- **Retries repetem operações não idempotentes:** limitar retry a falhas transitórias e testar repetição/duplicidade.
- **DLQ vira depósito sem operação:** documentar responsável, métricas, inspeção, correção e redrive.
- **Gateway concentra regras de domínio:** limitar YARP a roteamento e preocupações transversais.

## Referências

- [Microsoft — Microservices architecture style, Azure Architecture Center](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/microservices)
- [Microsoft — Microservices assessment and readiness](https://learn.microsoft.com/azure/architecture/guide/technology-choices/microservices-assessment)
- [Microsoft — Design a DDD-oriented microservice](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/ddd-oriented-microservice)
- [Microsoft — Data sovereignty per microservice](https://learn.microsoft.com/dotnet/architecture/microservices/architect-microservice-container-applications/data-sovereignty-per-microservice)
- [Microsoft — Integration event-based microservice communications](https://learn.microsoft.com/dotnet/architecture/microservices/multi-container-microservice-net-applications/integration-event-based-microservice-communications)
- [RabbitMQ — Reliability Guide](https://www.rabbitmq.com/docs/reliability)
- [RabbitMQ — Consumer Acknowledgements and Publisher Confirms](https://www.rabbitmq.com/docs/confirms)
- [MassTransit — Transactional Outbox](https://masstransit.io/documentation/patterns/transactional-outbox)
- ADR-0005 — Arquitetura inicial e limites entre camadas.
- ADR-0008 — Modelagem do domínio e ciclo de vida da solicitação.
- ADR-0011 — Observabilidade e operação local.
