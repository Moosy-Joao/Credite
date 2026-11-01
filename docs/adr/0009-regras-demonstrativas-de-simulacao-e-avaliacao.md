# ADR-0009 — Regras demonstrativas de simulação e avaliação

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Projeto Credite
- **Base de produto:** ADR-0007 — Visão do produto, atores e limites do Credite
- **Domínio:** ADR-0008 — Modelagem do domínio e ciclo de vida da solicitação
- **Escopo funcional:** RF-019 a RF-023 de `docs/REQUISITOS-MVP.md`

## Contexto

O MVP precisa oferecer ao Analista uma simulação reproduzível e explicável, preservar o resultado registrado e demonstrar cálculos monetários e testes de domínio. Ao mesmo tempo, o Credite é um projeto educacional: não pode produzir score financeiro, recomendação, aprovação ou negativa de crédito real.

A decisão precisa definir um método de amortização, limites técnicos para entradas, política de precisão/arredondamento, dados que ficam registrados e o significado de “avaliação”. O Banco Central descreve a Tabela Price como um método com prestações iguais quando a taxa não varia, e distingue esse comportamento do SAC, no qual a amortização é constante. A escolha aqui é didática e de implementação, não uma recomendação de produto financeiro.

## Decisão

Adotar uma simulação matemática didática baseada na **Tabela Price**, com taxa mensal fixa e sem encargos adicionais. A avaliação do MVP registra o cálculo e os parâmetros utilizados; **não calcula risco de crédito, score, capacidade de pagamento, elegibilidade nem decisão de aprovação/negativa**.

### 1. Método e fórmula

Entradas da simulação:

- Principal fictício (P), em reais.
- Taxa mensal fixa (i), representada como fração decimal (por exemplo, 1,50% = 0,0150).
- Número de períodos (n), em meses.

Para taxa positiva, a prestação teórica é:

`A = P × i / (1 − (1 + i)^(-n))`

Para taxa zero:

`A = P / n`

O cálculo deve ser feito com `decimal`, sem `float` ou `double` no caminho monetário. Para evitar conversão binária no expoente, a implementação pode calcular `(1 + i)^n` por multiplicação decimal iterativa e usar a forma algébrica equivalente da fórmula. Os limites definidos abaixo mantêm a potência dentro de uma faixa pequena e previsível.

### 2. Limites de entrada do simulador

Os limites são guardrails didáticos para manter os exemplos controlados. Não representam limites de concessão, parâmetros de mercado ou política de instituição financeira.

| Parâmetro | Limite do MVP |
|---|---:|
| Principal | R$ 100,00 a R$ 100.000,00 |
| Taxa mensal fixa | 0,00% a 5,00% a.m. |
| Precisão da taxa informada | 0,01 ponto percentual |
| Prazo | 1 a 60 meses |

Valores fora desses limites devem ser rejeitados antes do cálculo, com erro de validação compreensível. O simulador não deve truncar ou ajustar silenciosamente uma entrada inválida.

### 3. Precisão e política monetária

- Representar valores monetários com `decimal` e arredondar para duas casas decimais.
- Fixar explicitamente `MidpointRounding.AwayFromZero` em todos os pontos de arredondamento monetário; não depender do modo padrão implícito de `Math.Round`.
- Em cada período, calcular juros sobre o saldo inicial do período e arredondá-los para centavos.
- Arredondar a prestação calculada para centavos e manter esse valor nas primeiras (n-1) parcelas.
- Na última parcela, ajustar o pagamento para quitar exatamente o saldo remanescente acrescido dos juros do último período. Essa última parcela pode diferir alguns centavos das anteriores por causa dos arredondamentos.
- Calcular total de juros como soma dos juros arredondados de cada período e total pago como soma das parcelas efetivamente exibidas.
- Armazenar valores monetários com precisão/escala explícitas no EF Core/SQL Server, inicialmente `decimal(14,2)` para valores em reais. A taxa deve ter precisão/escala explícitas e compatíveis com os quatro dígitos da fração decimal (por exemplo, `decimal(6,4)`).

### 4. Conteúdo do resultado

A resposta da simulação deve apresentar, no mínimo:

- Método: Tabela Price demonstrativa.
- Principal, taxa mensal, prazo e identificador/versão da regra.
- Valor das parcelas; a indicação de que a última pode ter ajuste de centavos.
- Para cada período: número da parcela, saldo inicial, juros, amortização, prestação e saldo final.
- Total de juros e total pago.
- Aviso visível de que o cálculo é fictício e não considera tributos, IOF, seguros, tarifas, custos acessórios, datas reais, dias corridos, atraso ou regras de contratos financeiros.

### 5. Exemplo de referência para testes

Entrada: principal de R$ 1.000,00, taxa fixa de 2,00% a.m. e 12 períodos.

Resultado esperado, usando a política de arredondamento definida:

- Prestação teórica antes do arredondamento: aproximadamente R$ 94,5595966.
- Primeiras 11 parcelas: R$ 94,56.
- Última parcela ajustada: R$ 94,54.
- Total pago: R$ 1.134,70.
- Total de juros: R$ 134,70.
- Na primeira parcela: juros de R$ 20,00, amortização de R$ 74,56 e saldo final de R$ 925,44.
- Na segunda parcela: juros de R$ 18,51, amortização de R$ 76,05 e saldo final de R$ 849,39.

Caso de taxa zero: R$ 1.000,00 em 3 períodos resulta em R$ 333,33, R$ 333,33 e R$ 333,34, totalizando R$ 1.000,00 sem juros.

Esses exemplos são vetores de referência para testes unitários e não ofertas, propostas ou recomendações.

### 6. Significado de Assessment no MVP

`Assessment` representa um **registro imutável de uma execução demonstrativa**, não uma avaliação de risco financeiro. O registro deve preservar:

- Snapshot dos parâmetros de entrada usados.
- Identificador e versão da regra de cálculo.
- Resultado calculado, incluindo valores e cronograma.
- Autor e instante da execução.
- Identificador da solicitação associada.

Uma simulação ainda não confirmada pode ser recalculada como prévia. Ao registrar a avaliação, os parâmetros e resultados ficam preservados; uma alteração futura nas regras não recalcula avaliações históricas. O estado da solicitação avança de `InAnalysis` para `Evaluated` conforme ADR-0008. Reavaliação após `Evaluated`, sobrescrita ou retorno de estado não fazem parte desta decisão e exigem decisão futura explícita.

Não serão apresentados rótulos como “baixo risco”, “alto risco”, “aprovado”, “reprovado”, “elegível” ou equivalentes. A interface deve nomear o resultado como simulação/avaliação demonstrativa.

### 7. Testes obrigatórios

- Taxa zero e taxa positiva.
- Prazo mínimo e máximo.
- Principal mínimo e máximo.
- Entradas abaixo/acima dos limites e precisão excessiva.
- Saldo final exatamente zero após o ajuste da última parcela.
- Soma das amortizações igual ao principal.
- Total pago igual à soma das parcelas exibidas.
- Total de juros igual à soma dos juros por período.
- Casos de ponto médio para confirmar `AwayFromZero`.
- Exemplo de referência desta decisão.
- Persistência do snapshot e preservação de avaliações anteriores.

## Alternativas consideradas

- **SAC:** amortização constante e parcelas decrescentes. É matematicamente válido e descrito pelo Banco Central, mas a Price foi escolhida para o primeiro simulador porque apresenta uma prestação-base constante mais simples de explicar; suportar os dois métodos aumentaria o escopo.
- **Juros simples ou cálculo de parcela fixa sem cronograma:** mais simples, mas não demonstraria a composição de juros, amortização e saldo devedor ao longo dos períodos.
- **Modelo de score/risco fictício:** poderia gerar uma classificação visual, mas incentivaria interpretações de capacidade de pagamento ou decisão de crédito e exigiria variáveis e critérios artificiais. Foi rejeitado para manter o limite educacional inequívoco.
- **Adicionar IOF, seguros, tarifas e convenções de datas:** aumentaria a semelhança superficial com contratos reais, mas ampliaria o risco de transmitir precisão financeira que o projeto não oferece. Não será incluído.

## Consequências

### Positivas

- Cálculo determinístico, reproduzível e testável.
- Demonstração de aritmética decimal, arredondamento, persistência e versionamento de regras.
- Histórico pode explicar exatamente como cada resultado foi produzido.
- Fronteira explícita entre demonstração educacional e decisão financeira.

### Negativas / trade-offs

- O simulador não representa o custo efetivo total nem reproduz um contrato real.
- Apenas a Tabela Price é suportada no MVP.
- A última parcela pode variar alguns centavos em relação às demais para quitar o saldo após arredondamentos.
- Não haverá score ou classificação de risco, apesar do contexto de “análise” da aplicação.

### Riscos e ações de mitigação

- **Resultado interpretado como oferta real:** exibir aviso persistente na interface e no resultado exportado/compartilhado, se houver.
- **Mudança de regra altera histórico:** salvar versão e snapshot completo da avaliação.
- **Inconsistência de centavos:** usar `decimal`, política explícita e testes de reconciliação.
- **Entrada excessiva causa overflow ou cálculo abusivo:** aplicar os limites antes da execução e validar também no domínio/aplicação.
- **Dados verdadeiros inseridos por engano:** usar somente massas sintéticas e não coletar documentos ou dados bancários.

## Referências

- [Banco Central do Brasil — Glossário Simplificado de Termos Financeiros, verbetes PRICE (p. 55) e SAC (p. 62)](https://www.bcb.gov.br/content/cidadaniafinanceira/documentos_cidadania/Informacoes_gerais/glossario_cidadania_financeira.pdf)
- [Microsoft — Decimal.Round, .NET 10](https://learn.microsoft.com/pt-br/dotnet/api/system.decimal.round?view=net-10.0)
- [Microsoft — Propriedades de entidade e precisão/escala no EF Core](https://learn.microsoft.com/pt-br/ef/core/modeling/entity-properties)
- [Microsoft — Validação de modelos no ASP.NET Core MVC, .NET 10](https://learn.microsoft.com/pt-br/aspnet/core/mvc/models/validation?view=aspnetcore-10.0)
- ADR-0002 — Autenticação, autorização e dados fictícios.
- ADR-0003 — Estratégia de testes automatizados.
- ADR-0007 — Visão do produto, atores e limites do Credite.
- ADR-0008 — Modelagem do domínio e ciclo de vida da solicitação.
- `docs/REQUISITOS-MVP.md` — RF-019 a RF-023.
