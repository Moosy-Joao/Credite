# Planejamento e governança do Credite

## Estado atual

**Fase: planejamento e especificação. Implementação bloqueada.**

O objetivo desta fase é concluir requisitos, decisões de arquitetura, modelo de domínio, fluxos, critérios de aceite, estratégia de testes, segurança, observabilidade, documentação e plano incremental antes de construir funcionalidades.

## Regra de início obrigatório

Antes de iniciar qualquer implementação, o responsável deve:

1. Clonar `https://github.com/Moosy-Joao/Credite.git`.
2. Abrir `Credite.slnx` no Visual Studio Community.
3. Confirmar a instalação do SDK .NET 10.
4. Executar `dotnet restore Credite.slnx`.
5. Executar `dotnet build Credite.slnx`.
6. Executar `dotnet test Credite.slnx`.
7. Registrar os resultados e resolver os erros encontrados.
8. Confirmar explicitamente que essa etapa foi concluída.

Até essa confirmação, não iniciar implementação de domínio, endpoints, banco de dados, interface, mensageria ou microserviços. O planejamento e a documentação podem continuar.

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

## Definition of Done

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
