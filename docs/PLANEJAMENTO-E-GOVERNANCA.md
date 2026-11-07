# Planejamento e governança do Credite

## Estado atual

**Validação inicial concluída; desenvolvimento incremental autorizado conforme especificações e backlog aprovados.**

Em 2026-11-01, o responsável confirmou a clonagem do repositório, abertura de `Credite.slnx` no Visual Studio Community, SDK .NET 10 instalado e execução bem-sucedida de restore, build e testes.

Essa etapa está encerrada. Não solicitar novamente a confirmação nem tratá-la como bloqueio. Restore, build e testes continuam fazendo parte da validação normal de cada alteração técnica.

O Credite permanece como monólito modular no MVP, com SQL Server como banco oficial. Kafka, PostgreSQL, MongoDB e microsserviços pertencem inicialmente à trilha educacional isolada.

## Princípios de engenharia

- Começar simples e evoluir com evidências; não criar microserviços prematuramente.
- Preservar limites claros entre domínio, aplicação, infraestrutura e apresentação.
- A interface MVC comunica-se com o backend por HTTP; não acessa bancos diretamente.
- Regras de negócio não ficam em controllers, views ou infraestrutura.
- Toda funcionalidade relevante terá critérios de aceite e testes.
- Dependências externas devem resolver uma necessidade concreta.
- Preferir recursos nativos do .NET quando forem suficientes.
- Não usar dados pessoais, financeiros ou credenciais reais.
- Não simular uma decisão financeira real: regras e resultados são demonstrativos.
- Decisões arquiteturais relevantes devem ser registradas em ADRs.
- Cada alteração deve ser pequena, revisável e acompanhada de documentação quando necessário.

## Fluxo de trabalho

1. **Backlog de planejamento:** itens ainda não detalhados.
2. **Em especificação:** requisitos, opções e critérios de aceite em elaboração.
3. **Decisões pendentes:** questões que exigem decisão explícita.
4. **Pronto para execução:** especificação aprovada e dependências resolvidas; só pode avançar após o bloqueio inicial ser removido.
5. **Em andamento:** trabalho autorizado e em execução.
6. **Validação e revisão:** revisão técnica, testes e documentação.
7. **Concluído:** critérios de aceite atendidos e evidências registradas.

## Critérios para um cartão ficar pronto

- Objetivo e contexto compreensíveis.
- Escopo incluído e excluído.
- Dependências e riscos identificados.
- Critérios de aceite verificáveis.
- Regras de negócio documentadas.
- Contratos, modelos ou decisões relevantes descritos.
- Estratégia de teste definida.
- Documentação relacionada no repositório.
- Sem decisão arquitetural crítica em aberto.

## Critério de Aceite

Uma tarefa só pode ser concluída quando:
- todos os critérios de aceite foram atendidos;
- testes pertinentes foram executados e passaram;
- compilação não apresenta erros;
- logs e tratamento de falhas são adequados ao escopo;
- documentação foi atualizada;
- não há segredo ou dado real no repositório;
- revisão final foi realizada.

## Registro de decisões

Decisões duradouras devem ser registradas em `docs/adr/` usando o modelo `docs/adr/0000-template.md`. Cada ADR deve conter contexto, decisão, alternativas consideradas, consequências e estado (proposta, aceita, substituída ou rejeitada).

## Documentação central

- [README](../README.md)
- [Roadmap](ROADMAP.md)
- [Automações GitHub para .NET](AUTOMACOES-GITHUB-DOTNET.md)
- [Template de ADR](adr/0000-template.md)
- [Plano de aprendizado Back-End C#/.NET](PLANO-DE-APRENDIZADO-BACKEND.md)
- [ADR-0012 — Evolução para microserviços e mensageria](adr/0012-evolucao-para-microservicos-e-mensageria.md)
- [ADR-0013 — Laboratórios de PostgreSQL, MongoDB e Apache Kafka](adr/0013-laboratorios-postgresql-mongodb-kafka.md)
