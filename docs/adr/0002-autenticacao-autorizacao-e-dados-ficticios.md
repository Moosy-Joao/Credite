> **Nota de revisão (2026-11-07):** esta ADR foi escrita para o monólito com SQL Server e cookies compartilhados entre MVC/API. Identity agora é um microsserviço independente, dono de PostgreSQL próprio. As decisões sobre protocolo de autenticação, sessão da interface e autorização entre serviços ainda não foram aprovadas; não implementar as premissas de compartilhamento de cookies sem ADR-0015. Preservar o restante como histórico/requisitos de segurança, e revisar após a aprovação de ADR-0015.

# ADR-0002 — Autenticação, autorização e dados fictícios

- **Status:** Aceita
- **Data:** 2026-10-31

## Contexto

O Credite é uma aplicação educacional demonstrativa de gestão de solicitações de crédito. A interface administrativa será construída com ASP.NET Core MVC e consumirá a API por HTTP. A primeira versão será um monólito modular, com SQL Server e Entity Framework Core 10.

O sistema precisa autenticar usuários internos, proteger operações da API e distinguir contas de acesso dos registros de clientes fictícios. A solução deve ser adequada ao escopo inicial, utilizar recursos consolidados do ASP.NET Core e evitar a implementação manual de protocolos de identidade.

O Credite não deve armazenar dados pessoais, financeiros ou credenciais reais de clientes.

## Decisão

### Gerenciamento de identidade

- Utilizar ASP.NET Core Identity para gerenciamento de contas, senhas, funções e dados de autenticação.
- Persistir os dados de identidade no SQL Server por meio do Entity Framework Core 10.
- Não permitir cadastro público de contas.
- Criar a primeira conta administrativa por procedimento controlado, sem credenciais fixas no código ou no repositório.
- Utilizar o mecanismo de hash de senhas do Identity; nunca armazenar senhas em texto puro.

### Autenticação entre MVC e API

- Utilizar autenticação baseada em cookies.
- Configurar MVC e API para compartilhar o cookie de autenticação e as chaves do ASP.NET Core Data Protection.
- Manter consistentes entre as aplicações o esquema de autenticação, o nome do cookie, o nome do aplicativo Data Protection e o armazenamento/proteção das chaves.
- O MVC enviará a identidade autenticada nas chamadas HTTP à API.
- A API validará a autenticação e aplicará suas próprias regras de autorização. Não confiar apenas na origem da requisição.
- Exigir HTTPS e configurar cookies com atributos Secure, HttpOnly e SameSite apropriados ao fluxo.
- Proteger formulários MVC contra CSRF com antiforgery tokens.
- Endpoints de API não autenticados devem responder com códigos HTTP apropriados (401/403), sem redirecionamentos para páginas HTML de login.

### Autorização

Começar com funções do ASP.NET Core Identity e políticas de autorização nomeadas. As funções iniciais serão:

- **Administrador:** gerenciar usuários internos, funções e configurações administrativas; acesso às operações administrativas.
- **Analista:** consultar clientes e solicitações, executar avaliações demonstrativas e consultar históricos.
- **Operador:** cadastrar clientes fictícios, registrar solicitações e acompanhar seus estados.

A autorização será aplicada no servidor, especialmente nos endpoints da API. Ocultar elementos da interface não substitui autorização no backend. Exigir autenticação por padrão e liberar explicitamente apenas os endpoints públicos necessários, como health checks quando aplicável.

Não haverá perfil de cliente nem portal de autoatendimento no escopo inicial.

### Dados fictícios e segredos

- Usar exclusivamente dados demonstrativos inventados.
- Não cadastrar CPF, telefone, endereço, renda, documentos, dados financeiros ou credenciais pertencentes a pessoas reais.
- Identificar claramente os registros como fictícios na interface e na documentação.
- Separar contas de acesso dos registros de clientes fictícios.
- Usar User Secrets no desenvolvimento local e variáveis de ambiente ou gerenciador de segredos apropriado nos demais ambientes.
- Não versionar senhas, chaves, tokens, certificados ou outros segredos.

### Evolução futura

A API não será uma API pública para terceiros na primeira versão. Se surgirem clientes independentes, integrações externas ou necessidade de delegação de identidade, avaliar um provedor OAuth 2.0/OpenID Connect, como OpenIddict ou Microsoft Entra ID. Não implementar emissão manual de JWT nem construir um servidor de identidade próprio.

## Alternativas consideradas

1. **ASP.NET Core Identity com cookies compartilhados — escolhida.** Integra-se ao ecossistema ASP.NET Core, atende à interface MVC e à API sob controle do mesmo produto e evita introduzir um servidor de identidade separado.
2. **OAuth 2.0/OpenID Connect com OpenIddict.** Adequado para múltiplos clientes e APIs independentes, mas adiciona infraestrutura e complexidade que não são necessárias no primeiro incremento.
3. **Microsoft Entra ID.** Solução gerenciada e adequada a cenários corporativos, mas introduz dependência de um provedor externo e configuração adicional para o ambiente educacional.
4. **Tokens próprios ou JWT implementado manualmente.** Rejeitado por transferir ao projeto responsabilidades de segurança e protocolo sem benefício proporcional.

## Consequências

### Positivas

- Integração direta com ASP.NET Core MVC e ASP.NET Core Identity.
- Gerenciamento de contas e senhas baseado em componentes consolidados.
- Autorização explícita e centralizada no backend.
- Menor complexidade operacional para a primeira versão.
- Caminho de evolução definido caso surjam clientes externos.

### Negativas e cuidados

- O compartilhamento de cookies exige configuração consistente entre MVC e API.
- As chaves Data Protection precisam ser persistidas e protegidas adequadamente, especialmente fora do desenvolvimento local.
- Cookies exigem atenção a CSRF, HTTPS, SameSite e escopo de domínio.
- O modelo inicial de funções deverá ser revisto se surgirem permissões mais granulares ou múltiplos contextos independentes.

## Referências

- [ASP.NET Core Identity](https://learn.microsoft.com/aspnet/core/security/authentication/identity)
- [Compartilhamento de cookies entre aplicações ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/cookie-sharing)
- [Visão geral da autorização no ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authorization/introduction)
- [Proteção contra CSRF/XSRF no ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/anti-request-forgery)
- [Proteção de dados no ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/data-protection/introduction)
