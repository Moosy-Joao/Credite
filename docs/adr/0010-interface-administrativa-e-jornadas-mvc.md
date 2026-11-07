> **Nota de revisão (2026-11-07):** a escolha de MVC/Razor como interface administrativa permanece como referência de apresentação, mas as premissas de integração com uma única API, autenticação por cookie compartilhado e encaminhamento de identidade pertencem à arquitetura anterior. A integração da interface com Identity, Customer e Credit, a sessão e eventual BFF/Gateway dependem de ADR-0015. Não implementar os detalhes antigos até essa decisão ser aprovada.

# ADR-0010 — Interface administrativa MVC e jornadas do usuário

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Projeto Credite
- **Base de arquitetura:** ADR-0002 (identidade), ADR-0004 (HTTP), ADR-0005 (limites entre camadas) e ADR-0006 (qualidade)
- **Escopo funcional:** RF-001 a RF-026 de `docs/REQUISITOS-MVP.md`

## Contexto

O Credite precisa de uma interface administrativa para demonstrar o fluxo completo do MVP. A solução já escolheu ASP.NET Core MVC, Razor Views, Bootstrap e uma API separada. É necessário definir a navegação, as páginas por perfil, a integração HTTP, os modelos de apresentação e o comportamento de formulários, erros, estados vazios e acessibilidade.

O projeto é desenvolvido inicialmente por uma pessoa e não precisa de uma SPA, de um framework JavaScript adicional ou de um design system próprio. A interface deve manter o domínio e a persistência fora da camada Web, conforme ADR-0005.

## Decisão

Adotar **MVC server-rendered com Razor Views e Bootstrap 5.3**, com JavaScript mínimo e progressivo somente para aprimorar interações. A API permanece como única autoridade para regras de negócio, persistência e autorização dos recursos.

### 1. Estrutura de navegação

O layout autenticado terá navegação principal consistente e adaptada ao perfil:

- **Visão geral:** página inicial simples com atalhos para as tarefas disponíveis ao usuário e itens recentes/pendentes quando existirem dados. Não será um dashboard analítico.
- **Clientes:** consulta paginada, detalhes, cadastro e edição.
- **Solicitações:** consulta paginada e filtrável, cadastro, detalhes, estado atual e histórico.
- **Fila de análise:** acesso para Analista e Administrador, com solicitações elegíveis para análise.
- **Contas internas:** acesso exclusivo ao Administrador para consultar, criar, alterar perfil e desativar contas.
- **Conta e sessão:** identificação do usuário autenticado e encerramento de sessão.

Rotas MVC sugeridas (podem ser ajustadas às convenções de nomes adotadas durante a implementação):

| Área | Rotas/páginas |
|---|---|
| Acesso | `/Account/Login`, `/Account/AccessDenied` |
| Início | `/` |
| Clientes | `/Customers`, `/Customers/Create`, `/Customers/{id}`, `/Customers/{id}/Edit` |
| Solicitações | `/CreditRequests`, `/CreditRequests/Create`, `/CreditRequests/{id}` |
| Análise | `/Analysis/Queue`, `/CreditRequests/{id}/Assessment` |
| Administração | `/Admin/Users`, `/Admin/Users/Create`, `/Admin/Users/{id}/Edit` |

As rotas são uma organização da interface, não substituem os recursos e contratos REST definidos no ADR-0004.

### 2. Jornadas por ator

#### Administrador
1. Autenticar-se.
2. Acessar a visão geral.
3. Criar contas para Operador e Analista e atribuir funções.
4. Consultar, editar e desativar contas internas.
5. Acessar clientes, solicitações, fila de análise e histórico conforme as permissões do MVP.

#### Operador
1. Autenticar-se.
2. Consultar clientes existentes ou cadastrar um cliente fictício.
3. Abrir os detalhes e editar os dados permitidos.
4. Criar uma solicitação associada a um cliente.
5. Consultar solicitações, visualizar o estado e acompanhar o histórico.
6. Não visualizar comandos de análise nem executar avaliações.

#### Analista
1. Autenticar-se.
2. Consultar clientes e solicitações.
3. Abrir a fila de solicitações elegíveis.
4. Abrir uma solicitação em análise.
5. Conferir os parâmetros fictícios da Tabela Price e executar a simulação.
6. Revisar o cronograma, juros, amortização, total e aviso de finalidade demonstrativa.
7. Confirmar o registro da avaliação e consultar o snapshot salvo.
8. Não cadastrar nem editar clientes ou criar solicitações.

A matriz de autorização continua sendo a do documento de requisitos do MVP. A navegação oculta ações indisponíveis para reduzir confusão, mas isso é apenas uma medida de apresentação: a API precisa negar qualquer chamada não autorizada.

### 3. Responsabilidade da aplicação Web e integração

- `Credite.Web` continua referenciando apenas `Credite.Contracts` entre os projetos internos, conforme ADR-0005.
- Controllers MVC recebem requisições de página, aplicam políticas de acesso da interface e chamam a API por HTTP.
- Registrar clientes HTTP tipados ou nomeados para a API, com endereço-base vindo de configuração e timeout explícito.
- Encapsular chamadas HTTP em serviços de integração da camada Web por área funcional; não espalhar construção de URL, serialização e tratamento de erros por Controllers.
- Usar DTOs de `Credite.Contracts` nos limites HTTP e ViewModels próprios para cada página/formulário. Não reutilizar entidades do Domain ou modelos do EF Core na interface.
- Mapear ViewModels para DTOs e respostas da API para ViewModels dentro de `Credite.Web`.
- O MVC não referencia `Credite.Application`, `Credite.Domain` ou `Credite.Infrastructure`, não injeta `DbContext` e não consulta SQL Server diretamente.
- As chamadas MVC → API devem encaminhar a identidade autenticada da requisição atual conforme ADR-0002, usando o cookie compartilhado e a configuração comum de Data Protection. A credencial não pode ser persistida em armazenamento próprio nem registrada em logs.
- A API valida o cookie e executa sua própria autorização; não confiar em headers de identidade arbitrários nem apenas nas políticas aplicadas no MVC.
- Não introduzir JWT, bearer token manual ou API Gateway para esse fluxo.

### 4. ViewModels e páginas

Adotar ViewModels orientados a tela, sem criar uma hierarquia genérica de modelos. Exemplos iniciais:

- `CustomerListViewModel`, `CustomerDetailsViewModel`, `CustomerFormViewModel`.
- `CreditRequestListViewModel`, `CreditRequestDetailsViewModel`, `CreditRequestFormViewModel`.
- `AnalysisQueueViewModel`, `AssessmentFormViewModel`, `AssessmentResultViewModel`.
- `UserListViewModel`, `UserFormViewModel`.

Os campos exatos de Cliente e Solicitação continuam subordinados às decisões de atributos e validações do domínio. A interface não deve inventar CPF, renda ou outros dados pessoais/financeiros reais. Usar apenas valores sintéticos claramente identificados como fictícios.

### 5. Listagens, navegação e estados

- Listagens serão renderizadas no servidor e usarão a paginação do ADR-0004: `page=1`, `pageSize=20` por padrão e máximo de 100.
- Filtros relevantes serão enviados por query string, preservados ao navegar entre páginas.
- Toda ordenação deve ser determinística; não carregar coleções completas para paginar apenas no navegador.
- Estados de consulta: carregando durante a requisição, conteúdo, vazio e erro. Com renderização server-side, o estado vazio e o erro terão páginas/feedback explícitos; não adicionar polling nem carregamento infinito.
- Estado vazio explica o que não foi encontrado e apresenta uma ação principal somente quando o perfil pode executá-la.
- Depois de comandos bem-sucedidos, usar Post/Redirect/Get e mensagem de confirmação não sensível.
- Formulários inválidos mantêm os dados já digitados e mostram resumo de erros e mensagens junto aos campos.
- Falhas inesperadas exibem mensagem genérica e identificador de correlação quando disponível; stack trace, SQL e detalhes internos nunca são exibidos.
- Ações destrutivas ou terminais, como cancelar solicitação e desativar conta, exigem confirmação explícita e envio por POST. Não usar links GET para alterar estado.

### 6. Tratamento dos erros da API

Mapear os resultados do contrato Problem Details do ADR-0004 para uma experiência apropriada no MVC:

| Resposta API | Comportamento na interface |
|---|---|
| 400 | Reexibir formulário com erros de entrada/campos quando os detalhes permitirem |
| 401 | Encaminhar para login ou informar sessão expirada, preservando retorno seguro para rota local |
| 403 | Exibir página de acesso negado sem revelar detalhes de autorização |
| 404 | Exibir página de recurso não encontrado |
| 409 | Explicar conflito de estado/regra e orientar atualização da tela |
| 500 | Mensagem genérica, sem detalhes internos, com identificador de correlação quando disponível |

O MVC não deve converter uma falha da API em sucesso nem repetir automaticamente comandos POST cujo resultado ficou incerto. O contrato de erro e os códigos de status permanecem definidos pelo ADR-0004.

### 7. Formulários e segurança

- Model binding e validação server-side são obrigatórios; inspecionar `ModelState.IsValid` antes de enviar o comando à API.
- As validações locais servem à experiência da tela. A API e as camadas de domínio/aplicação continuam responsáveis por validar o contrato e as regras reais.
- Formularios que alteram estado devem usar antiforgery tokens; aplicar `AutoValidateAntiforgeryToken` globalmente às ações MVC não seguras, salvo exceção técnica justificada.
- Manter autenticação por cookie, HTTPS, Secure, HttpOnly e SameSite conforme ADR-0002.
- Usar codificação/Tag Helpers Razor padrão; não renderizar HTML recebido da API como conteúdo confiável.
- Botões e links devem refletir seus verbos: links navegam; formulários/POST executam comandos.
- Não colocar senhas, cookies, tokens, dados sensíveis ou corpos completos de formulários em logs.

### 8. Acessibilidade e responsividade

Manter WCAG 2.2 nível AA como objetivo definido no ADR-0006, com atenção especial às jornadas completas e formulários:

- Usar regiões semânticas, títulos hierárquicos, links de navegação e controles HTML nativos.
- Todo campo precisa de rótulo visível associado; instruções de formato e obrigatoriedade devem ser claras.
- Mensagens de erro devem ser textuais, associadas ao campo com `aria-describedby`, e não depender somente de cor ou ícone.
- Exibir resumo de erros no topo do formulário e permitir que o usuário localize o primeiro erro.
- Manter foco visível, ordem de tabulação lógica e operação completa por teclado.
- Contraste de texto e controles deve respeitar WCAG 2.2 AA; status devem ter texto além de cor.
- Garantir layout responsivo nas larguras comuns, sem esconder ações essenciais em telas menores.
- Usar tabelas semânticas com cabeçalhos para dados tabulares e links explícitos para abrir registros.

Bootstrap 5.3 será utilizado como ferramenta de layout e componentes, não como garantia de acessibilidade. A documentação do Bootstrap alerta que seus estilos/tooltips customizados de validação client-side não são acessíveis em todas as tecnologias assistivas. Por isso, o fluxo principal deve apresentar erros server-side acessíveis; realce client-side será apenas melhoria opcional e deve ser testado.

### 9. Testes e critérios de aceite da interface

- Testar integração dos Controllers/serviços MVC com a API simulada ou real de teste, incluindo sucesso, 400, 401, 403, 404, 409 e 500.
- Testar que cada perfil vê apenas os itens de navegação permitidos e que a API continua negando operações proibidas.
- Testar o fluxo Post/Redirect/Get, preservação de campos inválidos, feedback de sucesso e mensagens genéricas para falhas.
- Testar paginação, filtros e ordenação determinística.
- Testar antiforgery nos formulários de alteração.
- Fazer revisão manual com teclado, foco visível, zoom, viewport estreita e leitor de tela nas jornadas principais.
- Usar ferramentas automatizadas de acessibilidade como apoio, não como certificação de conformidade.
- Não considerar a interface pronta até o fluxo vertical do MVP funcionar do início ao fim com as permissões corretas.

## Alternativas consideradas

- **SPA separada (React/Angular/Blazor WebAssembly):** oferece interação rica e separação do frontend, mas acrescenta toolchain, contratos/client-state e preocupações de autenticação para uma aplicação administrativa pequena.
- **Razor Pages:** apropriado para aplicações focadas em páginas e formulários; MVC já está definido no stack inicial e oferece organização familiar por Controllers/Views.
- **MVC com Razor Views e JavaScript mínimo — escolhida:** usa o stack já aprovado, reduz infraestrutura e mantém o servidor responsável pelo fluxo de aplicação.
- **Acesso direto do MVC ao banco:** reduziria chamadas HTTP, mas quebraria o limite de arquitetura aprovado e duplicaria regras e autorização.

## Consequências

### Positivas

- Interface simples de executar, depurar e testar com Visual Studio e ferramentas .NET.
- Limites entre Web, Contracts, API e domínio ficam claros.
- Formulários e navegação funcionam sem exigir uma SPA.
- Segurança e validação permanecem no servidor.
- A experiência de portfólio demonstra consumo HTTP, mapeamento de DTOs, acessibilidade e tratamento de falhas.

### Negativas / trade-offs

- Uma interação exige ida ao servidor e, quando aplicável, uma chamada adicional à API.
- A interface tem menos interatividade imediata que uma SPA.
- Encaminhamento seguro de cookie entre MVC e API exige configuração e testes específicos.
- ViewModels e DTOs podem ter campos semelhantes, com custo de mapeamento intencional para preservar responsabilidades.

### Riscos e ações de mitigação

- **Duplicar regra de negócio em ViewModels/controllers:** manter validações de tela simples e validar regras na API/domínio.
- **MVC e API divergem na autorização:** testar os mesmos perfis no endpoint e na navegação.
- **Cookie não acompanha chamada MVC → API:** testar autenticação integrada e compartilhamento Data Protection no ambiente local e no ambiente de implantação.
- **Erro técnico vaza para a tela:** centralizar mapeamento de Problem Details e mensagens genéricas.
- **Bootstrap é confundido com conformidade de acessibilidade:** revisão manual e testes automatizados como parte da validação.

## Referências

- [Microsoft — Model validation in ASP.NET Core MVC, .NET 10](https://learn.microsoft.com/aspnet/core/mvc/models/validation?view=aspnetcore-10.0)
- [Microsoft — Prevent CSRF attacks in ASP.NET Core, .NET 10](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery?view=aspnetcore-10.0)
- [Microsoft — Cookie sharing between ASP.NET Core apps](https://learn.microsoft.com/aspnet/core/security/cookie-sharing)
- [W3C WAI — Labeling Controls](https://www.w3.org/WAI/tutorials/forms/labels/)
- [W3C WAI — Validating Input](https://www.w3.org/WAI/tutorials/forms/validation/)
- [W3C — WCAG 2.2, Focus Visible (2.4.7)](https://www.w3.org/WAI/WCAG22/Understanding/focus-visible.html)
- [Bootstrap 5.3 — Forms and validation](https://getbootstrap.com/docs/5.3/forms/validation/)
- ADR-0002 — Autenticação, autorização e dados fictícios.
- ADR-0004 — Contratos REST, erros e convenções HTTP.
- ADR-0005 — Arquitetura inicial e limites entre camadas.
- ADR-0006 — Requisitos não funcionais e critérios de qualidade.
- ADR-0009 — Regras demonstrativas de simulação e avaliação.
- `docs/REQUISITOS-MVP.md`.
