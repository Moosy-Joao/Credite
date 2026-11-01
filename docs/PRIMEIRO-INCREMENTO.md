# Primeiro incremento — Autenticação e autorização interna

- **Estado:** Especificação aprovada
- **Data:** 2026-11-01
- **Produto:** Credite
- **Base:** ADR-0002, ADR-0003, ADR-0004, ADR-0005, ADR-0010 e `docs/REQUISITOS-MVP.md`

## 1. Objetivo

Entregar a primeira fatia verificável do Credite: autenticação de usuários internos, sessão MVC integrada à API e autorização server-side por perfil. O incremento estabelece a fronteira de segurança necessária antes de expor operações de negócio.

É um incremento técnico-funcional deliberadamente pequeno. Não entrega ainda cadastro de clientes ou solicitações, nem conclui o fluxo vertical completo do MVP.

## 2. Resultado demonstrável

Ao final, deve ser possível:

1. Aplicar explicitamente as migrations em um SQL Server local.
2. Provisionar a primeira conta Administrador por procedimento local controlado.
3. Entrar na interface MVC com essa conta e encerrar a sessão.
4. Acessar páginas protegidas enquanto autenticado.
5. Chamar um endpoint protegido da API por meio do MVC, com a identidade autenticada encaminhada e validada pela API.
6. Comprovar que endpoints protegidos retornam 401 sem autenticação e 403 quando o usuário autenticado não tem o perfil exigido.
7. Confirmar que não há cadastro público, credenciais fixas, senha em texto puro ou autorização baseada apenas na interface.

## 3. Escopo incluído

### 3.1 Persistência e Identity

- Configurar ASP.NET Core Identity com EF Core 10 e SQL Server.
- Persistir contas e roles no banco aprovado no ADR-0001.
- Criar roles iniciais: `Administrador`, `Analista` e `Operador`.
- Configurar cookies compartilhados entre MVC e API, incluindo esquema, nome de cookie, ApplicationName e Data Protection key ring, conforme ADR-0002.
- Configurar autenticação e autorização por padrão; liberar explicitamente apenas endpoints públicos previstos, como health checks.
- Exigir HTTPS e atributos de cookie Secure, HttpOnly e SameSite adequados ao fluxo.
- Configurar lockout temporário após tentativas malsucedidas, com parâmetros explícitos e testados.
- Não permitir auto-registro, login de clientes, autenticação social, JWT manual ou provedor externo de identidade.

### 3.2 Provisionamento inicial do Administrador

A primeira conta não pode depender de cadastro público nem de credenciais embutidas no código.

Decisão de provisionamento para o incremento:

- Disponibilizar um comando explícito de execução local no host API, por exemplo `--bootstrap-admin`, executado somente após a migration do banco.
- O e-mail/login e a senha inicial serão lidos de configuração secreta (User Secrets em desenvolvimento; secret manager/variáveis protegidas em outros ambientes). Senha nunca será argumento de linha de comando.
- O comando cria a conta Administrador e garante a existência das três roles iniciais.
- O comando é seguro para repetição: se o Administrador inicial já existir, não redefine senha nem modifica a conta; informa que o provisionamento já foi realizado e termina sem alteração.
- Se houver estado parcial ou conflito (por exemplo, usuários existentes sem uma condição segura para concluir o bootstrap), aborta com erro claro e sem sobrescrever contas.
- Não criar endpoint HTTP de bootstrap, senha padrão, usuário padrão, seed automático na inicialização ou credenciais em appsettings versionado.
- Após o provisionamento, remover o segredo temporário do armazenamento local quando não for mais necessário.

O formato exato do comando pode ser implementado no composition root existente, sem criar um projeto CLI separado neste incremento. A lógica de provisionamento deve ser isolada e testável.

### 3.3 Interface MVC

- Página de login com validação server-side e mensagens genéricas que não revelem se uma conta existe.
- Logout por POST com antiforgery.
- Páginas administrativas protegidas por autenticação.
- Exibir o perfil atual e uma navegação mínima adequada ao estado autenticado.
- Não implementar ainda CRUD de contas, recuperação de senha, edição de perfil ou desativação de contas.
- O MVC encaminha o cookie autenticado nas chamadas HTTP à API; não confia em headers de identidade inventados.
- Redirecionamentos de login são responsabilidade da interface MVC; a API responde 401/403 sem redirecionar para HTML.

### 3.4 API

- Configurar autenticação pelo cookie compartilhado.
- Configurar políticas nomeadas/roles para proteger endpoints.
- Criar apenas os endpoints mínimos necessários ao primeiro incremento, sem expor endpoints artificiais de teste no ambiente normal.
- Manter os health checks públicos somente conforme configuração explícita.
- Responder com 401 para não autenticado e 403 para autenticado sem autorização, seguindo ADR-0004.
- Não implementar ainda endpoints de clientes, solicitações, avaliações ou administração de contas.

## 4. Fora do escopo

- CRUD administrativo de contas (RF-003 a RF-006).
- Cadastro, consulta e edição de clientes (RF-008 a RF-012).
- Solicitações, estados, histórico e simulação (RF-013 a RF-026).
- Recuperação de senha por e-mail, MFA, login social, OIDC/OAuth externo.
- Portal público ou auto-registro.
- Autorização granular por recurso/ownership.
- Auditoria corporativa completa.
- Microserviços, RabbitMQ, MassTransit, Redis, YARP, Outbox e Saga.
- Testes E2E de navegador com Playwright.
- Implantação em produção ou provisionamento automatizado de infraestrutura.

## 5. Critérios de aceite verificáveis

### CA-01 — Banco e migrations

- A migration de Identity é aplicada explicitamente em banco SQL Server descartável/local.
- As tabelas e relacionamentos necessários são criados sem migration automática na inicialização da API.
- A aplicação inicia com banco previamente preparado.
- A aplicação não usa EF Core InMemory nem SQLite como substituto do SQL Server nos testes de persistência.

### CA-02 — Provisionamento inicial

- O comando cria o primeiro Administrador e as roles esperadas.
- O comando não contém nem imprime senha em logs.
- Reexecutar o comando não redefine senha nem duplica contas/roles.
- Ausência de segredo, estado parcial ou conflito encerra o processo sem criar uma conta insegura.
- Nenhuma credencial inicial fica no Git ou em argumentos de processo.

### CA-03 — Login e logout

- Credenciais válidas autenticam a conta ativa.
- Credenciais inválidas não autenticam e recebem mensagem genérica.
- Logout invalida a sessão do navegador.
- Formulário de logout exige antiforgery.
- Cookies possuem atributos de segurança configurados.
- Nenhuma rota pública de cadastro de usuário é disponibilizada.

### CA-04 — Integração MVC → API

- MVC encaminha o cookie da sessão atual para a API em chamada HTTP server-side.
- API valida o cookie compartilhado e identifica o usuário autenticado.
- A chamada sem cookie válido retorna 401.
- A chamada com cookie válido, mas sem o perfil exigido, retorna 403.
- A chamada com perfil permitido recebe a resposta esperada.
- MVC e API compartilham corretamente o Data Protection key ring em configuração de teste.

### CA-05 — Autorização por perfil

- Roles Administrador, Analista e Operador são criadas.
- Políticas/roles são aplicadas nos endpoints protegidos da API.
- A autorização não depende da ocultação de links ou botões no MVC.
- Health checks públicos são exceções explícitas; nenhuma outra rota é liberada por acidente.

### CA-06 — Segurança e privacidade

- Senhas são gerenciadas pelo ASP.NET Core Identity e nunca armazenadas em texto puro.
- Não há credenciais padrão, segredos versionados, tokens ou senhas em logs.
- Mensagens de login não permitem enumeração de contas.
- Lockout temporário é aplicado após o limite configurado de falhas.
- Não são utilizados dados pessoais reais.

### CA-07 — Testes e CI

- Testes unitários cobrem a lógica isolada do provisionamento, quando aplicável.
- Testes de integração exercitam API e MVC com `WebApplicationFactory`.
- Testes de persistência e Identity usam SQL Server descartável via Testcontainers for .NET.
- Cobrir login válido/inválido, logout, cookie compartilhado, 401, 403, roles, bootstrap idempotente e antiforgery.
- Os testes usam banco isolado e migrations aplicadas desde um banco vazio.
- GitHub Actions executa restore, build, unit tests e integration tests em runner com Docker.
- Testes E2E de navegador ficam adiados; a navegação é validada por testes de integração MVC e revisão manual.

## 6. Dependências e pré-condições

- ADR-0001: SQL Server e migrations.
- ADR-0002: Identity, cookies, roles, segredos e dados fictícios.
- ADR-0003: xUnit, WebApplicationFactory, SQL Server descartável e CI.
- ADR-0004: contratos HTTP, 401/403 e Problem Details.
- ADR-0005: limites entre Web, API, Application, Domain, Infrastructure e Contracts.
- ADR-0010: interface MVC e antiforgery.
- Fase 0: clone, abertura de Credite.slnx no Visual Studio Community, SDK .NET 10, restore, build, test e confirmação explícita.

A Fase 0 continua sendo bloqueio absoluto para implementação. Esta especificação não autoriza criar projetos, migrations, endpoints ou código.

## 7. Sequência sugerida de trabalho após liberação

1. Confirmar a Fase 0 e registrar resultados.
2. Validar estrutura e referências da solução.
3. Configurar DbContext/Identity em Infrastructure e composição na API.
4. Criar migrations e aplicá-las explicitamente ao SQL Server local.
5. Implementar roles e provisionamento inicial seguro.
6. Implementar login/logout MVC e cookie compartilhado.
7. Proteger API e validar políticas/roles.
8. Implementar testes unitários e integração com SQL Server descartável.
9. Integrar o job de CI com o estágio de testes de integração.
10. Revisar segurança, mensagens de erro, logs e documentação.

Cada etapa deve ser pequena, com build/teste antes de avançar. A ordem pode ser ajustada se uma dependência técnica comprovada exigir.

## 8. Próximos incrementos candidatos

Após a conclusão deste incremento, decompor o MVP em fatias de negócio:

1. Cadastro e consulta de clientes fictícios (após decisão dos atributos e validações de Customer).
2. Criação e consulta de solicitações (após decisão dos atributos e validações de CreditRequest).
3. Transições de estado e histórico.
4. Simulação Price e registro de Assessment.
5. Administração de contas e demais jornadas MVC que não couberem no primeiro incremento.
6. Fluxo completo de aceite e testes transversais.

A ordem e os critérios detalhados desses incrementos devem ser definidos antes de movê-los para execução.

## 9. Referências

- [Microsoft — Introduction to Identity in ASP.NET Core (.NET 10)](https://learn.microsoft.com/aspnet/core/security/authentication/identity?view=aspnetcore-10.0)
- [Microsoft — Share authentication cookies between ASP.NET Core apps](https://learn.microsoft.com/aspnet/core/security/cookie-sharing)
- [Microsoft — Integration tests in ASP.NET Core (.NET 10)](https://learn.microsoft.com/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
- [Testcontainers for .NET — SQL Server module](https://dotnet.testcontainers.org/modules/mssql/)
- ADR-0001 a ADR-0012.
