# ADR-0007 — Visão do produto, atores e limites do Credite

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Projeto Credite

## Contexto

O Credite é um projeto educacional e de portfólio que demonstra práticas de engenharia de software com C# e .NET. Para orientar o escopo do MVP, as jornadas e a modelagem do domínio, é necessário estabelecer uma visão de produto, identificar os atores do cenário e explicitar os limites funcionais e de responsabilidade.

O cenário de negócio é fictício. Não há pesquisa de campo que comprove que os problemas descritos representam todas as instituições financeiras; portanto, eles são hipóteses de modelagem para fins didáticos, não afirmações sobre operações financeiras reais.

As decisões de autenticação e autorização, dados fictícios, arquitetura inicial e requisitos não funcionais já estão registradas nos ADRs-0002, 0005 e 0006.

## Decisão

Adotar o Credite como uma **plataforma administrativa educacional e demonstrativa de gestão de solicitações de crédito fictícias**.

### Visão

Ser uma plataforma administrativa demonstrativa que permita registrar, acompanhar e simular solicitações de crédito fictícias de ponta a ponta, com regras explícitas, resultados compreensíveis, histórico rastreável e acesso controlado por função.

### Problema abordado

Em um cenário organizacional fictício, informações de clientes, solicitações, simulações e avaliações podem ficar dispersas entre registros e etapas diferentes. Isso dificulta acompanhar o estado de uma solicitação e compreender como um resultado demonstrativo foi produzido.

O Credite centralizará esse fluxo em uma aplicação, permitindo acompanhar registros, mudanças de estado e resultados de simulações fictícias. Essa descrição representa uma hipótese de modelagem do projeto, não uma afirmação empírica sobre instituições financeiras.

### Atores

Os perfis operacionais iniciais serão:

- **Operador:** registra e mantém clientes fictícios, inicia solicitações e acompanha seus estados.
- **Analista:** consulta solicitações e executa ou examina avaliações demonstrativas, compreendendo os resultados das regras simuladas.
- **Administrador:** gerencia contas internas, atribui perfis e controla o acesso ao sistema.

O cliente fictício é um registro de negócio, não uma conta de autenticação. Não terá login ou portal próprio na primeira versão. Recrutadores, avaliadores e visitantes do portfólio são stakeholders externos do projeto, não atores operacionais do sistema.

Os perfis e o modelo de autenticação permanecem subordinados ao ADR-0002; este ADR define seus objetivos funcionais, não substitui as regras de autorização técnica.

### Benefícios pretendidos

- Centralizar o fluxo demonstrativo de clientes e solicitações.
- Permitir acompanhar estados e histórico de solicitações.
- Tornar os resultados das simulações compreensíveis por meio de regras fictícias explícitas.
- Demonstrar competências de engenharia de software em uma aplicação .NET coerente, testável e evolutiva.

### Limites do produto

O Credite:

- Não realiza análise real de crédito, score financeiro ou avaliação real de capacidade de pagamento.
- Não aprova nem nega crédito de verdade.
- Utiliza exclusivamente dados fictícios e sintéticos; não inserir CPF, documentos, credenciais, contas bancárias ou informações financeiras reais.
- Não consulta bancos, birôs de crédito ou serviços financeiros externos.
- Não contrata empréstimos, libera valores, processa pagamentos ou gera obrigações financeiras.
- Não oferece cadastro público nem portal de clientes na primeira versão.
- Não declara conformidade regulatória ou certificação financeira.

As simulações serão demonstrativas, transparentes e explicáveis. Seus resultados não podem ser apresentados como recomendações ou decisões financeiras reais.

### Critérios de sucesso em nível de produto

A visão será considerada demonstrada quando:

1. For possível percorrer um cenário completo com clientes e solicitações fictícias.
2. Os perfis tiverem responsabilidades distintas e as permissões forem verificadas no servidor.
3. Os resultados de simulação puderem ser compreendidos a partir das regras fictícias utilizadas.
4. O histórico permitir acompanhar o que aconteceu com uma solicitação.
5. O funcionamento puder ser demonstrado e verificado por testes automatizados.

Esses critérios são objetivos de produto em alto nível. O escopo funcional definitivo, os fluxos e os critérios de aceite do MVP serão especificados separadamente.

## Alternativas consideradas

- **Sistema para operação financeira real:** descartado por exigir dados reais, integrações, controles e responsabilidades fora do propósito educacional e do escopo individual do projeto.
- **Demonstração genérica de arquitetura .NET:** descartada por não oferecer um processo de negócio suficientemente coerente para exercitar modelagem, regras e fluxos ponta a ponta.
- **Plataforma administrativa demonstrativa de crédito fictício:** escolhida por oferecer um domínio compreensível, permitir demonstrar competências técnicas relevantes e manter limites seguros e viáveis.

## Consequências

### Positivas

- Visão, público operacional e limites explícitos para orientar o MVP.
- Coerência entre o cenário de negócio, os perfis do ADR-0002 e a arquitetura do ADR-0005.
- Base para especificar casos de uso, domínio, estados e critérios de aceite.
- Redução do risco de apresentar simulações como decisões financeiras reais.

### Negativas / trade-offs

- O cenário não valida necessidades de instituições financeiras reais.
- A restrição a dados e integrações fictícias reduz o realismo operacional, mas é intencional para o propósito educacional.
- A definição de visão não determina, por si só, todas as funcionalidades do MVP.

### Riscos e ações de mitigação

- **Hipóteses apresentadas como fatos de mercado:** identificar o cenário como fictício e não atribuir os problemas descritos a instituições reais sem pesquisa.
- **Simulação confundida com análise real:** exibir avisos claros e manter regras demonstrativas explicáveis.
- **Dados reais inseridos em demonstrações ou testes:** utilizar apenas dados sintéticos e revisar seeds, fixtures, logs e documentação.
- **Crescimento prematuro do escopo:** detalhar o MVP em decisão própria e priorizar um fluxo vertical pequeno e verificável.

## Referências

- ADR-0002 — Autenticação, autorização e dados fictícios.
- ADR-0005 — Arquitetura inicial e limites entre camadas.
- ADR-0006 — Requisitos não funcionais e critérios de qualidade.
- [GOV.UK — Understand users and their needs](https://www.gov.uk/service-manual/user-research/start-by-learning-user-needs)
- [Roman Pichler — Product Vision Board](https://www.romanpichler.com/tools/product-vision-board/)
- [The Scrum Guide](https://scrumguides.org/scrum-guide.html)
