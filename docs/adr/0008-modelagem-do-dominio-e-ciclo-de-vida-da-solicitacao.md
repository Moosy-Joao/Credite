# ADR-0008 — Modelagem do domínio e ciclo de vida da solicitação

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Projeto Credite
- **Base de produto:** ADR-0007 — Visão do produto, atores e limites do Credite
- **Escopo funcional:** Requisitos funcionais e escopo do MVP

## Contexto

O MVP precisa representar clientes fictícios, solicitações, avaliações demonstrativas e o histórico das principais operações. O domínio deve proteger as regras de consistência e o ciclo de vida das solicitações, sem antecipar complexidade desnecessária nem transformar cada tabela de persistência em um conceito de domínio.

O Credite começa como monólito modular com Clean Architecture. O domínio não deve depender de ASP.NET Core, EF Core, Identity ou serviços externos. A autorização dos usuários é responsabilidade das camadas de entrada e aplicação; a validade das operações conforme o estado do negócio pertence ao domínio.

Todos os dados e resultados permanecem fictícios. O modelo não representa análise, score, aprovação ou negativa financeira real.

## Decisão

Adotar DDD tático moderado, com entidades que protegem invariantes e limites de agregados definidos pelas necessidades de consistência transacional. Evitar padrões e abstrações sem necessidade demonstrada.

### Conceitos e agregados

1. **Customer:** entidade e raiz de um agregado próprio, representando o cadastro de um cliente fictício.
2. **CreditRequest:** entidade e raiz de um agregado próprio, responsável pelo ciclo de vida da solicitação e por proteger suas transições válidas.
3. **Assessment:** registro de avaliação demonstrativa associado à solicitação. Avaliações anteriores devem ser preservadas; uma nova avaliação não pode sobrescrever silenciosamente uma anterior.
4. **CreditRequestHistoryEntry:** registro dos eventos relevantes da solicitação, incluindo criação, transições de estado e avaliações registradas.
5. **Money:** Value Object candidato para valores monetários fictícios, caso os casos de uso de simulação necessitem desse conceito. Seus detalhes e regras de arredondamento serão definidos na decisão de simulação.

A solicitação referencia o cliente por CustomerId, sem manter uma referência de objeto navegável entre raízes de agregados. Os identificadores tipados (CustomerId, CreditRequestId, AssessmentId) podem ser adotados se trouxerem clareza e segurança de tipos durante a implementação; não são requisito obrigatório desta decisão.

A lista acima define os conceitos e limites iniciais. Não fixa ainda os atributos completos, campos obrigatórios, formatos de identificadores ou mapeamentos de persistência.

### Ciclo de vida inicial de CreditRequest

Estados aprovados:

- **PendingAnalysis:** solicitação registrada e aguardando análise.
- **InAnalysis:** análise iniciada por um Analista autorizado.
- **Evaluated:** uma avaliação demonstrativa foi registrada.
- **Cancelled:** solicitação cancelada; estado terminal no fluxo aprovado.

Transições aprovadas:

| Estado de origem | Estado de destino | Condição |
|---|---|---|
| Criação | PendingAnalysis | Solicitação válida vinculada a cliente existente |
| PendingAnalysis | InAnalysis | Operação de início de análise válida |
| InAnalysis | Evaluated | Avaliação demonstrativa registrada com seus dados de rastreabilidade |
| PendingAnalysis | Cancelled | Cancelamento permitido |
| InAnalysis | Cancelled | Cancelamento permitido |

Não há transição aprovada a partir de Evaluated ou Cancelled. A possibilidade de reavaliar uma solicitação já avaliada permanece em aberto para decisão explícita futura. Não se deve inferir uma transição de retorno nem substituir uma avaliação anterior.

Os nomes em inglês representam os identificadores de estado a serem considerados na implementação; os rótulos apresentados aos usuários poderão ser localizados em português.

### Invariantes iniciais

- Uma solicitação deve estar vinculada a um cliente fictício existente.
- A associação entre solicitação e cliente não pode ser trocada arbitrariamente após a criação.
- Uma solicitação não pode pular etapas do ciclo de vida.
- Uma avaliação só pode ser registrada quando a solicitação estiver em estado elegível (InAnalysis).
- Uma avaliação registrada não pode ser silenciosamente sobrescrita.
- Toda transição relevante deve gerar um registro de histórico.
- Cada evento de histórico deve identificar o autor e o instante da operação.
- Uma solicitação cancelada não pode retornar ao fluxo normal.
- Operações inválidas para o estado atual devem ser rejeitadas pelo domínio, independentemente de validações da API ou da interface.

A raiz CreditRequest deve expor operações explícitas para mudanças de estado, como iniciar análise, registrar avaliação e cancelar. Não se deve permitir que a API altere livremente uma propriedade de estado.

### Responsabilidades por camada

- **Credite.Domain:** entidades, Value Objects, estados, transições e invariantes; sem dependências de frameworks ou infraestrutura.
- **Credite.Application:** casos de uso, coordenação das operações, autorização contextual, transações e obtenção da identidade do ator.
- **Credite.Infrastructure:** persistência SQL Server, mapeamentos EF Core e implementações técnicas.
- **Credite.Api:** autenticação, autorização HTTP, validação de entrada e exposição dos casos de uso.
- **Credite.Contracts:** DTOs de requisição e resposta.
- **Credite.Web:** apresentação administrativa e consumo HTTP da API.

A autorização por perfil (Administrador, Analista e Operador) permanece conforme ADR-0002 e a matriz do MVP. O domínio não deve referenciar ASP.NET Core Identity nem conhecer roles, claims ou cookies. A Application/API garantem que o ator pode solicitar a operação; o domínio garante que a operação é válida no estado atual.

## Alternativas consideradas

- **Modelo anêmico com regras em serviços:** simples para CRUD, mas deixa invariantes e transições dispersas e vulneráveis a caminhos alternativos de alteração.
- **DDD tático moderado:** protege regras relevantes com entidades e agregados, mantendo custo compatível com o MVP. É a opção escolhida.
- **DDD abrangente com muitos padrões, eventos e abstrações:** pode ser útil em domínios mais complexos, mas seria prematuro para o escopo atual.
- **Um único agregado para cliente, solicitações, avaliações e histórico:** facilitaria algumas operações transacionais, mas acoplaria o ciclo de vida da solicitação ao cadastro do cliente e ampliaria desnecessariamente o agregado.
- **Referência direta entre agregados:** conveniente para navegação de objetos, mas aumenta o acoplamento entre raízes. Foi escolhida a referência por identificador.

## Consequências

### Positivas

- Regras do ciclo de vida ficam protegidas no domínio.
- Cliente e solicitação têm responsabilidades e ciclos de vida independentes.
- Avaliações e eventos relevantes podem ser preservados para rastreabilidade.
- Autorização de usuário permanece separada das invariantes de negócio.
- A modelagem é compatível com monólito modular e pode evoluir sem exigir microserviços.

### Trade-offs e limitações

- Operações que envolvem mais de um agregado precisam ser coordenadas pela Application e respeitar as transações definidas para o caso de uso.
- O histórico funcional exige que os casos de uso registrem eventos relevantes de maneira consistente com a mudança de estado.
- Os campos definitivos, unicidade, exclusão, retenção, regras de reavaliação e detalhes monetários ainda precisam de decisões específicas.

## Decisões futuras relacionadas

- Atributos, validações e política de exclusão de Customer.
- Atributos e validações de CreditRequest.
- Fórmulas, parâmetros, arredondamento e casos-limite das avaliações demonstrativas.
- Reavaliação de solicitações no estado Evaluated.
- Política de histórico, auditoria e retenção.
- Mapeamentos EF Core, concorrência e transações.
- Contratos REST e jornadas da interface administrativa.

## Referências

- ADR-0001 — SQL Server e estratégia de persistência.
- ADR-0002 — Autenticação, autorização e dados fictícios.
- ADR-0003 — Estratégia de testes automatizados.
- ADR-0004 — Contratos REST, erros e convenções HTTP.
- ADR-0005 — Arquitetura inicial e limites entre camadas.
- ADR-0006 — Requisitos não funcionais e critérios de qualidade.
- ADR-0007 — Visão do produto, atores e limites do Credite.
- Requisitos funcionais e escopo do MVP.
- [Microsoft — Domain model layer validations](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-model-layer-validations).
- [Microsoft — Tactical DDD to design microservices](https://learn.microsoft.com/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/tactical-domain-driven-design).
