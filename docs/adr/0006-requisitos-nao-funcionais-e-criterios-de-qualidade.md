# ADR-0006 — Requisitos não funcionais e critérios de qualidade

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Projeto Credite

## Contexto

O Credite é uma aplicação educacional e de portfólio, desenvolvida inicialmente por uma pessoa, com API ASP.NET Core, interface MVC, SQL Server e dados inteiramente fictícios. O projeto precisa de critérios de qualidade verificáveis, mas proporcionais ao escopo e à infraestrutura disponível.

Os requisitos devem orientar implementação, revisão e testes sem impor metas arbitrárias de desempenho, disponibilidade ou escala antes de existirem cenários representativos.

## Decisão

Adotar requisitos não funcionais proporcionais ao estágio do projeto, organizados por segurança, privacidade, desempenho, confiabilidade, acessibilidade, observabilidade e manutenibilidade.

### 1. Segurança e privacidade

- Utilizar ASP.NET Core Identity, autenticação e autorização conforme o ADR-0002.
- Aplicar autorização no backend/API em todas as operações protegidas; ocultar controles na interface não substitui autorização no servidor.
- Utilizar HTTPS e configurar cookies com Secure, HttpOnly e SameSite apropriado ao fluxo de autenticação compartilhada.
- Aplicar proteção antiforgery nos formulários MVC que utilizam autenticação por cookie.
- Armazenar segredos de desenvolvimento em User Secrets; em outros ambientes, utilizar variáveis de ambiente ou gerenciador de segredos apropriado.
- Não registrar senhas, cookies, tokens, segredos ou informações pessoais/financeiras sensíveis nos logs.
- Não expor stack traces, consultas SQL ou detalhes internos em respostas destinadas a usuários em ambientes de produção.
- Utilizar exclusivamente dados fictícios. Não inserir CPF, documentos, credenciais, contas bancárias ou informações financeiras reais.
- Utilizar OWASP ASVS 5.0.0 nível 1 como checklist inicial de requisitos de segurança aplicáveis e OWASP Top 10:2025 como referência de conscientização. Isso não representa declaração de conformidade, certificação ou auditoria formal.

### 2. Desempenho

- Manter paginação nas consultas de coleções, conforme ADR-0004: página inicial 1, tamanho padrão 20 e máximo 100.
- Evitar consultas sem limite, carregamento desnecessário de relacionamentos e operações síncronas de I/O quando houver APIs assíncronas apropriadas.
- Medir latência, percentis, taxa de erros, consumo de memória e comportamento das consultas SQL nos fluxos principais.
- Não estabelecer metas numéricas de latência, throughput ou concorrência antes de definir cenários de carga e ambiente representativos.
- Otimizar com base em medições, não em suposições.
- Não adicionar Redis ou outros componentes de cache sem necessidade demonstrada por medição e caso de uso.

### 3. Disponibilidade e confiabilidade

- Não estabelecer SLA ou exigência de disponibilidade 24/7 para a fase inicial.
- Não exigir alta disponibilidade, múltiplas réplicas, Kubernetes ou implantação distribuída no estágio inicial.
- Quando o banco ou uma dependência crítica estiver indisponível, a aplicação deve falhar de forma controlada e não apresentar uma operação como concluída.
- Preservar integridade dos dados por meio de transações, restrições e tratamento consistente de falhas.
- Disponibilizar Health Checks adequados à aplicação e às dependências relevantes.
- Distinguir, quando necessário, verificações de vida do processo e prontidão para receber requisições, conforme o ambiente de execução.

### 4. Acessibilidade e usabilidade

- Adotar WCAG 2.2 nível AA como objetivo de acessibilidade para as jornadas principais da interface MVC.
- Considerar navegação por teclado, foco visível, ordem lógica de foco, rótulos associados a campos, mensagens de erro compreensíveis, contraste adequado e uso que não dependa exclusivamente de cores.
- Manter as telas principais utilizáveis em diferentes tamanhos de tela.
- Realizar revisão manual dos fluxos principais e utilizar ferramentas automatizadas como apoio, sem tratar a aprovação automatizada como prova de conformidade integral.

### 5. Observabilidade e diagnóstico

- Utilizar logs estruturados para eventos relevantes e exceções.
- Propagar identificadores de correlação para permitir investigar requisições entre MVC e API.
- Acompanhar métricas de requisições, duração e erros nos fluxos principais.
- Disponibilizar Health Checks para aplicação e dependências relevantes.
- Não registrar dados sensíveis.
- A estratégia e operação local de OpenTelemetry, OTLP, Health Checks e Aspire Dashboard standalone estão definidas no ADR-0011, sem exigir uma plataforma permanente de APM.

### 6. Manutenibilidade e testabilidade

- Seguir a estratégia de testes definida no ADR-0003: testes unitários e de integração, com SQL Server real para testes de persistência e isolamento de dados.
- O pipeline de CI deverá executar restore, build, testes unitários e testes de integração conforme a estratégia aprovada.
- Utilizar versões estáveis e suportadas do .NET e de seus pacotes, compatíveis com .NET 10 e EF Core 10.
- Revisar e atualizar dependências periodicamente, priorizando correções de segurança e compatibilidade.
- Não estabelecer percentual mínimo arbitrário de cobertura de código; cobertura será indicador complementar, não critério isolado de qualidade.
- Preservar separação de responsabilidades e dependências definida no ADR-0005.

## Alternativas consideradas

- **Requisitos de produção empresarial:** introduziriam metas de SLA, alta disponibilidade e escala incompatíveis com a fase educacional e sem cenários que as justifiquem.
- **Ausência de requisitos formais:** reduziria documentação inicial, mas deixaria segurança, acessibilidade, desempenho e confiabilidade sujeitos a interpretações inconsistentes.
- **Requisitos proporcionais e verificáveis:** escolhidos por equilibrar qualidade técnica, aprendizagem, capacidade de execução individual e evolução futura.

## Consequências

### Positivas

- Critérios explícitos para revisar segurança, acessibilidade e confiabilidade.
- Menor risco de adicionar infraestrutura prematuramente.
- Base para testes e decisões técnicas futuras.
- Metas de desempenho poderão ser definidas com evidências e cenários reais.
- Qualidade proporcional ao propósito educacional e de portfólio.

### Negativas / trade-offs

- Não haverá metas numéricas de desempenho e disponibilidade na fase inicial.
- WCAG AA e ASVS nível 1 exigem revisão contínua e não são garantidos apenas pela escolha de frameworks ou ferramentas.
- A adoção de controles de segurança e testes aumenta o esforço de desenvolvimento, mas é necessária para demonstrar práticas responsáveis.

### Riscos e ações de mitigação

- **Checklist tratado como certificação:** documentar que ASVS é referência de verificação e que não há declaração de conformidade formal.
- **Acessibilidade verificada somente por ferramentas:** incluir testes manuais por teclado e revisão das jornadas principais.
- **Otimização prematura:** exigir medição e cenário reproduzível antes de introduzir cache ou metas rígidas.
- **Dados reais inseridos acidentalmente:** utilizar dados sintéticos em exemplos, seeds, testes, demonstrações e documentação.
- **Logs vazarem informações:** revisar campos registrados e incluir testes/revisões para evitar segredos e dados sensíveis.

## Referências

- [OWASP Application Security Verification Standard (ASVS)](https://owasp.org/www-project-application-security-verification-standard/)
- [OWASP Top 10:2025](https://owasp.org/Top10/2025/)
- [W3C — Web Content Accessibility Guidelines (WCAG) 2.2](https://www.w3.org/TR/WCAG22/)
- [Microsoft — ASP.NET Core security](https://learn.microsoft.com/aspnet/core/security/)
- [Microsoft — ASP.NET Core performance best practices](https://learn.microsoft.com/aspnet/core/fundamentals/best-practices)
- [Microsoft — Health checks in ASP.NET Core](https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks)
- [Microsoft — .NET observability](https://learn.microsoft.com/dotnet/core/diagnostics/observability-with-otel)
- ADR-0011 — Observabilidade e operação local.
- [Microsoft — .NET and .NET Core support policy](https://dotnet.microsoft.com/platform/support/policy/dotnet-core)
- ADR-0002 — Autenticação, autorização e dados fictícios.
- ADR-0003 — Estratégia de testes automatizados.
- ADR-0004 — Contratos REST, erros e convenções HTTP.
- ADR-0005 — Arquitetura inicial e limites entre camadas.
