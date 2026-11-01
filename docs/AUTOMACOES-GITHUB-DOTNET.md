# Automações GitHub recomendadas para o Credite (.NET)

- **Estado:** Plano aprovado; automações adicionais ainda não ativadas
- **Data:** 2026-11-01
- **Contexto:** projeto individual, C# 14 / .NET 10, SQL Server, xUnit, GitHub Actions
- **Pré-condição:** Fase 0 precisa ser concluída e confirmada antes de validar ou alterar pipelines e código.

Este documento define quais automações serão adotadas, em que ordem, com quais permissões e critérios. Não é evidência de que uma automação foi ativada ou executada. O workflow inicial já existente é o único pipeline versionado no momento; seu sucesso ainda não foi validado localmente.

## 1. Princípios

1. Priorizar GitHub Actions, .NET SDK e recursos nativos do GitHub.
2. Cada workflow recebe somente as permissões necessárias.
3. Nenhum workflow de CI recebe secrets ou permissão de escrita no repositório.
4. Atualizações de dependências geram PRs para revisão; sem auto-merge.
5. Checks obrigatórios só serão configurados depois de uma execução confiável e identificável.
6. Não automatizar deploy, publicação, release ou alterações de produção sem ambiente e política aprovados.
7. Não ativar automações que possam contornar o bloqueio de implementação da Fase 0.

## 2. Matriz de adoção

| Prioridade | Automação | Decisão | Momento |
|---|---|---|---|
| P0 | GitHub Actions CI | Manter o workflow atual e evoluir para testes unitários e integração SQL Server | Após Fase 0 e validação da solução |
| P0 | Pull Request template | Adotar template Markdown enxuto | Junto da configuração de colaboração |
| P0 | Dependabot NuGet + GitHub Actions | Adotar atualizações semanais limitadas, sem auto-merge | Depois de confirmar manifests/projetos |
| P1 | EditorConfig + dotnet format | Adotar convenções mínimas e verificação no CI | Depois da primeira compilação local e baseline |
| P1 | CodeQL default setup | Preferir configuração padrão gerenciada pelo GitHub | Depois que houver código C# funcional |
| P1 | Issue templates | Adotar templates Markdown para bug, funcionalidade e tarefa técnica | Junto do PR template |
| P1 | Ruleset/proteção de main | Exigir PR e CI; zero aprovações obrigatórias por ser projeto individual | Depois de CI estável |
| P1 | Secret scanning / push protection | Habilitar se disponível para a visibilidade/plano do repositório | Revisão das configurações Security |
| P2 | CODEOWNERS | Não adotar agora; não há equipe com ownership por diretório | Reavaliar se houver contribuidores |
| P2 | Releases/changelog automáticos | Adiar até haver política de versão e cadência de releases | Fora do primeiro ciclo |
| P2 | Deploy contínuo | Não adotar; não há ambiente de implantação definido | Nova decisão quando houver ambiente |

## 3. CI — GitHub Actions

### Estado atual

O arquivo `.github/workflows/dotnet.yml` já configura .NET 10 e executa:

- checkout;
- setup-dotnet 10.0.x;
- `dotnet restore Credite.slnx`;
- `dotnet build Credite.slnx --no-restore --configuration Release`;
- `dotnet test Credite.slnx --no-build --configuration Release`.

Triggers atuais: push e pull request direcionados a `main`. O workflow tem `contents: read`. Ainda não há evidência de execução bem-sucedida da solução e os testes de integração definidos no ADR-0003 ainda precisam ser implementados.

### Decisão para a evolução do CI

Manter um workflow principal de validação .NET, com um job de validação inicialmente. Evitar separar em múltiplos jobs enquanto isso não trouxer paralelismo ou isolamento útil.

Configuração-alvo:

- Eventos: `push` para `main`, `pull_request` com base em `main` e `workflow_dispatch` para execução manual.
- `permissions: contents: read` no nível do workflow; nenhuma permissão de escrita.
- `concurrency` por workflow e ref, com cancelamento de execuções antigas da mesma branch.
- `timeout-minutes` explícito e razoável após medir a duração real do pipeline.
- Checkout sem persistência de credenciais quando não houver necessidade de fazer push a partir do workflow.
- Configurar .NET 10 por `actions/setup-dotnet`.
- Restaurar uma vez, compilar em Release sem restaurar novamente e testar sem recompilar.
- Quando o projeto de integração existir, o `dotnet test Credite.slnx` deve executar os dois projetos de teste; a etapa de integração utilizará SQL Server descartável via Testcontainers for .NET.
- Runner `ubuntu-latest` é suficiente inicialmente; Docker deve estar disponível para Testcontainers.
- Não adicionar cache NuGet até existir evidência de ganho e estratégia estável de arquivos de lock/cache.
- Não publicar artefatos de cada build por padrão. Publicar TRX/logs somente se facilitar diagnóstico real.
- Não introduzir deploy, secrets de cloud ou permissões de publicação no CI.

### Segurança dos workflows

- Preferir actions fixadas em SHA completo e verificado, não apenas tags mutáveis. Dependabot será responsável por propor atualização dessas referências.
- Não usar `pull_request_target` para executar código de branches externas.
- Não interpolar entrada não confiável diretamente em scripts shell.
- Não executar comandos com privilégios ou segredos desnecessários.
- Manter `GITHUB_TOKEN` com menor privilégio; o CI de build/teste não precisa escrever no repositório.
- Não usar PAT pessoal para checkout, build ou testes.
- Definir `permissions` explicitamente em todos os workflows futuros.

A atualização do workflow só ocorrerá após a Fase 0, e a CI será considerada confiável após uma execução completa bem-sucedida em push e PR.

## 4. Dependabot

Adotar Dependabot para dois ecossistemas:

1. **NuGet:** monitorar manifests dos projetos em `/src/*` e `/tests/*`.
2. **GitHub Actions:** monitorar referências de actions em `/.github/workflows`.

Frequência semanal, preferencialmente segunda-feira. Limitar PRs abertos para evitar ruído (até cinco PRs NuGet e três PRs Actions).

Política de atualização:

- Agrupar apenas atualizações patch de baixo risco por ecossistema.
- Manter atualizações minor e major em PRs separados para facilitar avaliação de mudanças.
- Atualizações de segurança devem permanecer identificáveis e receber prioridade.
- Não habilitar auto-merge nem aprovação automática.
- Cada PR precisa passar CI e receber revisão manual de changelog, breaking changes, compatibilidade com .NET 10/EF Core 10 e resultado dos testes.
- Não atualizar dependências diretamente na branch principal.
- Revisar periodicamente se os limites e grupos geram PRs grandes demais.

Configuração-alvo de referência (a criar após confirmar a estrutura de projetos):

```yaml
version: 2
updates:
  - package-ecosystem: nuget
    directories:
      - "/src/*"
      - "/tests/*"
    schedule:
      interval: weekly
      day: monday
    open-pull-requests-limit: 5
    groups:
      nuget-patch:
        patterns:
          - "*"
        update-types:
          - patch

  - package-ecosystem: github-actions
    directory: /
    schedule:
      interval: weekly
      day: monday
    open-pull-requests-limit: 3
    groups:
      actions-patch:
        patterns:
          - "*"
        update-types:
          - patch
```

O exemplo não foi criado como `.github/dependabot.yml` ainda. O caminho `directories` aceita múltiplos diretórios e padrões globais conforme a referência oficial do Dependabot; confirmar que os manifests futuros continuam dentro desses diretórios quando a estrutura mudar.

Separadamente, habilitar Dependabot alerts e security updates se estiverem disponíveis nas configurações de segurança do repositório. O arquivo de version updates, por si só, não substitui alertas de vulnerabilidade.

## 5. EditorConfig e dotnet format

Adotar `.editorconfig` na raiz, com `root = true`, UTF-8, newline consistente, remoção de trailing whitespace e indentação de quatro espaços para C#. Regras de estilo devem ser mínimas no início; não impor preferências cosméticas ou analyzers excessivamente rígidos sem benefício demonstrável.

Fluxo:

1. Depois da Fase 0, executar `dotnet format Credite.slnx` localmente para identificar e corrigir o baseline.
2. Revisar as alterações antes do commit.
3. Executar localmente `dotnet format Credite.slnx --verify-no-changes --no-restore`.
4. Adicionar a mesma verificação ao CI depois que o baseline estiver limpo.
5. Não usar `dotnet format` no CI em modo de correção; CI deve apenas validar e falhar quando houver diferenças.
6. Analyzers adicionais podem ser configurados por severidade após observar os avisos reais.

Não transformar toda sugestão de estilo em erro de build. A prioridade é consistência e detecção de problemas, sem bloquear o aprendizado com centenas de regras não relacionadas ao domínio.

## 6. CodeQL e segurança de dependências

Adotar **CodeQL default setup**, em vez de manter um workflow CodeQL customizado, se a configuração estiver disponível para o repositório.

- O repositório público no GitHub.com é elegível ao code scanning; confirmar visibilidade e disponibilidade nas configurações atuais antes de habilitar.
- Para C#, o default setup usa build mode `none`; o CodeQL resolve dependências com base nos arquivos do projeto e metadados disponíveis.
- Habilitar depois que o código C# funcional estiver presente, para que a análise produza sinais úteis.
- Verificar a primeira análise e revisar alertas; não tratar ausência de alertas como prova de segurança.
- Não manter simultaneamente setup padrão e workflow avançado concorrentes.
- CodeQL complementa, mas não substitui testes, revisão, análise de dependências e práticas seguras.

Habilitar secret scanning e push protection se disponíveis para o plano e visibilidade do repositório. Verificar o comportamento e eventuais exclusões antes de confiar na proteção. Segredos continuam proibidos no Git; usar User Secrets localmente e secret manager/variáveis protegidas nos demais ambientes.

## 7. Templates de Issues e Pull Requests

Adotar templates Markdown simples, sem formulários extensos, adequados a um projeto individual.

### Issue — bug

Campos: comportamento observado, comportamento esperado, passos de reprodução, ambiente/versão, logs sem segredos e critério de correção.

### Issue — funcionalidade/requisito

Campos: problema, objetivo, atores, escopo incluído/excluído, regras, critérios de aceite, dependências, riscos e testes.

### Issue — tarefa técnica

Campos: motivação, contexto, alternativas, impacto arquitetural, riscos, critérios de aceite e validação.

### Pull Request

Checklist enxuto:

- Resumo e motivação.
- Issue/card relacionado.
- Escopo e comportamento alterado.
- Testes executados e resultados.
- Efeito sobre segurança, autorização e dados fictícios.
- Documentação/ADR atualizado quando necessário.
- Migrations revisadas, quando aplicável.
- Nenhum segredo ou dado real incluído.
- CI aprovada.

Não exigir que o autor faça uma revisão de si mesmo nem adicionar CODEOWNERS sem múltiplos responsáveis.

## 8. Proteção da branch main

Adotar uma regra de proteção/ruleset para `main` quando o CI estiver estável:

- Exigir Pull Request para alterações em `main`.
- Exigir o check de CI real correspondente ao workflow de validação .NET; selecionar o nome somente após uma execução bem-sucedida aparecer no GitHub.
- Exigir zero aprovações obrigatórias, pois o projeto tem um único mantenedor e autoaprovação não agrega revisão independente.
- Bloquear force push e exclusão da branch.
- Preferir squash merge para manter histórico conciso; manter commits de trabalho em português e PRs pequenos.
- Não exigir linear history ou signed commits nesta fase, a menos que o repositório passe a ter necessidade explícita.
- Não configurar bypass amplo de administradores; qualquer exceção deve ser consciente e documentada.
- Não exigir checks de CodeQL como bloqueio até a análise estar habilitada, estável e produzir um status check confiável.
- Não tornar o check obrigatório antes de confirmar que ele executa em todos os PRs relevantes; caso contrário, pode bloquear a própria manutenção.

Branches de trabalho devem ser curtas e focadas, por exemplo `feature/<assunto>`, `fix/<assunto>` e `docs/<assunto>`. Não criar uma política de Git Flow completa para um projeto individual.

As proteções de branch estão disponíveis em repositórios públicos com GitHub Free; confirmar a disponibilidade atual na configuração do repositório.

## 9. O que não adotar agora

- Auto-merge de Dependabot.
- Bots de revisão automática ou comentários gerados sem necessidade.
- CODEOWNERS para uma equipe inexistente.
- Release Drafter, semantic-release ou changelog automático antes de definir versionamento.
- Workflow de deploy sem ambiente aprovado.
- Publicação de pacotes NuGet.
- Cache de CI sem medição/lockfiles.
- Matriz de builds multi-OS/múltiplas versões de SDK sem requisito.
- Testes E2E de navegador antes das jornadas MVC estabilizarem.
- Pipeline separado por módulo/serviço; o MVP é monólito modular.
- Ferramentas externas que dupliquem recursos já disponíveis no GitHub ou .NET SDK.

## 10. Sequência de implantação após liberação da Fase 0

1. Validar restore, build e testes localmente e corrigir a estrutura.
2. Executar CI atual em push/PR e verificar resultados.
3. Definir e aplicar `.editorconfig`; executar `dotnet format` e estabelecer baseline.
4. Adicionar templates de Pull Request e Issues.
5. Criar `.github/dependabot.yml` e habilitar alertas/updates disponíveis.
6. Introduzir testes de integração com SQL Server via Testcontainers e garantir que CI os execute com Docker.
7. Habilitar CodeQL default setup e revisar a primeira análise.
8. Habilitar secret scanning/push protection, se disponível.
9. Configurar ruleset de `main` exigindo PR e CI.
10. Reavaliar custos, ruído e manutenção após alguns ciclos de PR.

Essa ordem evita tornar checks obrigatórios antes de eles serem confiáveis e reduz a chance de bloquear o desenvolvimento por uma configuração incompleta.

## 11. Critérios de aceite do planejamento

- Workflow atual foi inspecionado e suas limitações registradas.
- Estratégia de CI para unitários e integração SQL Server está definida.
- Permissões mínimas, concorrência e segurança de Actions foram especificadas.
- Dependabot tem ecossistemas, diretórios, frequência, limites e política de revisão definidos.
- EditorConfig/dotnet format têm sequência de baseline e verificação definida.
- CodeQL default setup e secret scanning têm pré-condições de elegibilidade definidas.
- Templates de Issues/PR estão especificados.
- Ruleset de main está especificado sem exigir autoaprovação.
- Ordem de implantação e itens explicitamente adiados estão documentados.
- README e Trello apontam para este documento.

## 12. Referências oficiais

- [GitHub — .NET build and test with Actions](https://docs.github.com/actions/tutorials/build-and-test-code/net)
- [Microsoft — GitHub Actions overview for .NET](https://learn.microsoft.com/dotnet/devops/github-actions-overview)
- [GitHub — Secure use reference for Actions](https://docs.github.com/actions/reference/security/secure-use)
- [GitHub — Workflow syntax and GITHUB_TOKEN permissions](https://docs.github.com/actions/reference/workflows-and-actions/workflow-syntax)
- [GitHub — Workflow concurrency](https://docs.github.com/actions/how-tos/write-workflows/choose-when-workflows-run/control-workflow-concurrency)
- [GitHub — Dependabot options reference](https://docs.github.com/code-security/reference/supply-chain-security/dependabot-options-reference)
- [GitHub — Code scanning default setup](https://docs.github.com/code-security/how-tos/find-and-fix-code-vulnerabilities/configure-code-scanning/configure-code-scanning)
- [GitHub — CodeQL build modes for C#](https://docs.github.com/code-security/reference/code-scanning/codeql/build-options-for-compiled-languages)
- [GitHub — Available rules for rulesets](https://docs.github.com/repositories/configuring-branches-and-merges-in-your-repository/managing-rulesets/available-rules-for-rulesets)
- [GitHub — Protected branches and plan availability](https://docs.github.com/repositories/configuring-branches-and-merges-in-your-repository/managing-protected-branches)
- [Microsoft — dotnet format](https://learn.microsoft.com/dotnet/core/tools/dotnet-format)
- [Testcontainers for .NET — SQL Server](https://dotnet.testcontainers.org/modules/mssql/)
- ADR-0003 — Estratégia de testes automatizados.
- ADR-0006 — Requisitos não funcionais e critérios de qualidade.
- ADR-0011 — Observabilidade e operação local.

## Estado final

O planejamento está aprovado e documentado. O workflow atual permanece como está; nenhuma automação adicional foi ativada nesta etapa. A implementação da configuração proposta depende da conclusão e confirmação da Fase 0.
