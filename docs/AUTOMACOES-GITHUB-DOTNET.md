# Automações GitHub úteis para o Credite (.NET)

Pesquisa baseada em documentação oficial do GitHub e Microsoft. A seleção prioriza automações simples, baixo custo de manutenção e ganho prático para um projeto individual de portfólio.

## Recomendações priorizadas

| Prioridade | Automação | Benefício | Esforço | Decisão |
|---|---|---|---|---|
| P0 | GitHub Actions: restore, build e test | Detecta quebra de compilação e testes em push/PR | Baixo | Manter e validar |
| P0 | Pull Request template | Padroniza contexto, testes, checklist e vínculo com issue | Baixo | Implementar após planejamento |
| P0 | Dependabot para NuGet e GitHub Actions | Abre PRs de atualização de dependências e workflows | Baixo | Configurar com limites e revisão |
| P1 | dotnet format + .editorconfig | Padroniza estilo e detecta diferenças de formatação | Baixo | Adicionar ao CI após definir convenções |
| P1 | CodeQL default setup | Análise estática de segurança para C# | Baixo a médio | Avaliar elegibilidade e ativar |
| P1 | Issue templates | Padroniza requisitos, bugs e tarefas técnicas | Baixo | Implementar |
| P1 | Proteção da branch main / rulesets | Impede merge sem verificações obrigatórias | Baixo | Configurar quando regras de CI estiverem estáveis |
| P2 | CODEOWNERS | Solicita revisão conforme áreas do código | Baixo | Útil se houver mais contribuidores |
| P2 | Release notes / changelog automatizado | Facilita rastrear entregas | Médio | Adiar até existir cadência de releases |

## 1. CI com GitHub Actions

O repositório já contém um workflow inicial em `.github/workflows/dotnet.yml`, que configura o SDK .NET 10, restaura, compila e executa testes.

Próximas melhorias a especificar:
- validar workflow em push e pull request;
- separar restore, build e test para facilitar diagnóstico;
- publicar resultados de testes como artefatos quando isso agregar valor;
- adicionar cache NuGet somente depois de introduzir arquivos de lock e medir benefício;
- tornar checks obrigatórios na branch principal quando estiverem confiáveis.

Documentação:
- https://docs.github.com/pt/actions/tutorials/build-and-test-code/net
- https://learn.microsoft.com/pt-br/dotnet/devops/github-actions-overview

## 2. Dependabot

Dependabot pode abrir pull requests para atualizar dependências vulneráveis e versões regulares. Também pode atualizar referências de GitHub Actions usadas nos workflows.

Proposta:
- NuGet: verificar semanalmente.
- GitHub Actions: verificar semanalmente.
- Não habilitar auto-merge inicialmente.
- Revisar changelog, breaking changes e resultado de CI antes de aceitar cada PR.
- Agrupar atualizações de baixo risco quando fizer sentido; manter atualizações de segurança identificáveis.

O arquivo `.github/dependabot.yml` deverá ser criado após confirmar a estrutura final dos projetos e dependências.

Documentação:
- https://docs.github.com/en/code-security/tutorials/secure-your-dependencies/dependabot-quickstart
- https://docs.github.com/pt/code-security/how-tos/secure-your-supply-chain/secure-your-dependencies/configure-version-updates
- https://docs.github.com/en/code-security/concepts/supply-chain-security/dependabot-version-updates

## 3. dotnet format e EditorConfig

O comando nativo `dotnet format` aplica ou verifica regras de estilo da solução e lê configurações do `.editorconfig`.

Proposta:
- definir convenções C# em `.editorconfig`;
- executar localmente `dotnet format Credite.slnx` para corrigir;
- executar no CI `dotnet format Credite.slnx --verify-no-changes` para validar sem modificar;
- evitar regras excessivamente rígidas antes de o padrão do projeto estar acordado.

Documentação:
- https://learn.microsoft.com/pt-br/dotnet/core/tools/dotnet-format

## 4. CodeQL

CodeQL analisa o código em busca de vulnerabilidades e erros. Para repositórios públicos, o GitHub documenta a configuração padrão de code scanning. Para C#, o setup padrão usa build mode `none` e resolve dependências com base nos arquivos de projeto e metadados disponíveis.

Proposta:
- avaliar e habilitar a configuração padrão pelo painel Security do GitHub;
- verificar se a primeira análise concluiu;
- revisar alertas antes de alterar o código;
- evitar manter workflow avançado próprio se o setup padrão atender.

Documentação:
- https://docs.github.com/en/code-security/how-tos/find-and-fix-code-vulnerabilities/configure-code-scanning/configure-code-scanning
- https://docs.github.com/en/code-security/concepts/code-scanning/setup-types
- https://docs.github.com/en/code-security/reference/code-scanning/codeql/build-options-for-compiled-languages

## 5. Templates de issues e pull requests

Templates reduzem retrabalho e garantem que cada alteração tenha contexto, critérios de aceite, testes e documentação.

Proposta de templates:
- Bug: comportamento atual, esperado, reprodução, logs e ambiente.
- Feature/requisito: problema, objetivo, escopo, regras, critérios de aceite e dependências.
- Tarefa técnica: motivação, alternativas, impacto, risco e validação.
- Pull request: resumo, issue relacionada, decisões, testes executados, checklist e documentação alterada.

Documentação:
- https://docs.github.com/en/pull-requests/reference/managing-and-standardizing-pull-requests

## Ordem recomendada de adoção

1. Validar o workflow CI que já existe, depois da primeira compilação local.
2. Definir `.editorconfig` e validar `dotnet format`.
3. Adicionar templates de issue e pull request.
4. Configurar Dependabot para NuGet e GitHub Actions.
5. Ativar CodeQL default setup e revisar os resultados.
6. Tornar checks obrigatórios na branch principal.
7. Avaliar automações adicionais somente quando houver necessidade demonstrada.

## Restrições

- Não aceitar atualizações automáticas sem revisão.
- Não habilitar auto-merge de Dependabot no início.
- Não adicionar ferramentas de terceiros quando o SDK ou o GitHub já resolverem.
- Não adicionar pipeline de deploy antes de existir ambiente e estratégia de publicação.
- Não automatizar release, versionamento ou geração de changelog antes de definir política de releases.

## Estado

Este documento é uma proposta de planejamento. Nenhuma nova automação descrita aqui foi ativada, além do workflow CI inicial que já existe no repositório.
