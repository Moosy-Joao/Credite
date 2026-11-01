# ADR-0011 — Observabilidade e operação local

- **Estado:** Aceita
- **Data:** 2026-10-31
- **Responsáveis:** Projeto Credite
- **Decisões relacionadas:** ADR-0003 (testes), ADR-0005 (limites entre camadas), ADR-0006 (qualidade), ADR-0009 (simulação) e ADR-0010 (interface MVC)

## Contexto

O Credite terá dois hosts ASP.NET Core (Web MVC e API), SQL Server e chamadas HTTP entre a interface e a API. Mesmo sendo um projeto individual e inicialmente local, é necessário conseguir diagnosticar falhas de autenticação, chamadas HTTP, persistência e simulações, distinguindo falhas da aplicação de indisponibilidade de dependências.

A instrumentação deve permitir acompanhar uma requisição de ponta a ponta sem introduzir prematuramente uma plataforma de monitoramento de produção, múltiplos serviços auxiliares ou complexidade que não contribui para o MVP.

O ADR-0006 já estabelece logs estruturados, correlação, métricas, tracing e Health Checks como objetivos; esta decisão especifica como aplicá-los.

## Decisão

Adotar **APIs nativas de diagnóstico do .NET e OpenTelemetry**, com exportação OTLP opcional para desenvolvimento local, logs em console e Health Checks do ASP.NET Core. Não adotar neste estágio uma plataforma permanente de APM, dashboards de produção, alertas ou orquestração com Aspire.

### 1. Logs estruturados

- Utilizar `Microsoft.Extensions.Logging.ILogger<T>` nos componentes que precisarem registrar eventos.
- Preferir templates estruturados com propriedades nomeadas em vez de interpolar mensagens.
- Registrar eventos operacionais relevantes: início/fim de requisições, falhas inesperadas, indisponibilidade de dependências, falhas de integração e conclusão de operações importantes.
- Quando uma mudança de estado ou uma avaliação for confirmada, manter o histórico funcional definido no ADR-0008 como fonte de rastreabilidade de negócio. Logs não substituem esse histórico.
- Em desenvolvimento, manter saída estruturada no console; habilitar formato JSON quando apropriado ao host/ambiente e disponibilizar exportação OTLP de logs apenas quando houver endpoint configurado.
- Usar níveis coerentes: Information para eventos operacionais relevantes, Warning para situações recuperáveis ou entradas conflitantes, Error para falhas que impedem uma operação e Debug/Trace somente para diagnóstico temporário.
- Incluir contexto de correlação (TraceId/SpanId quando disponíveis), nome do serviço, ambiente e EventId nos registros.
- Não registrar senhas, cookies, tokens, chaves, connection strings, cabeçalhos de autenticação, corpos completos de requisição, documentos pessoais ou valores financeiros de solicitações/avaliações.
- Não registrar dados de clientes fictícios em logs de rotina; usar identificadores técnicos apenas quando necessários e sem transformá-los em dimensões de métricas.

### 2. Métricas

Instrumentar inicialmente as métricas automáticas disponíveis para ASP.NET Core, HttpClient, runtime .NET e SqlClient quando aplicável.

Acompanhar, no mínimo:
- Quantidade e duração de requisições HTTP, segmentadas por serviço, método, rota normalizada e resultado/status.
- Falhas em chamadas HTTP de saída e duração das dependências.
- Indicadores gerais do runtime, como uso de CPU/memória e comportamento do garbage collector, quando fornecidos pela instrumentação padrão.

Métricas próprias da aplicação só devem ser adicionadas quando responderem a uma pergunta operacional concreta, por exemplo, quantidade de simulações efetivamente registradas ou transições concluídas. Elas devem usar dimensões de baixa cardinalidade (resultado/tipo/estado), nunca identificadores de cliente, solicitação, usuário, valores monetários ou texto livre.

Não definir metas numéricas de latência, throughput, disponibilidade ou alertas antes de medir um cenário representativo, conforme ADR-0006.

### 3. Rastreamento e correlação

- Utilizar `System.Diagnostics.ActivitySource` e `Activity` para spans específicos da aplicação quando o tracing automático não explicar suficientemente a operação.
- Utilizar o contexto W3C Trace Context e a propagação automática de trace context entre ASP.NET Core e `HttpClient` instrumentado.
- Instrumentar a entrada HTTP nos hosts Web/API, chamadas Web → API e consultas SQL relevantes através dos pacotes de instrumentação oficiais do OpenTelemetry.
- Criar spans próprios para operações de aplicação relevantes, como executar e registrar uma simulação, sem registrar parâmetros monetários ou dados pessoais como atributos.
- Preservar o mesmo nome de serviço e metadados de ambiente por host em todos os exporters.
- Não criar middleware, cabeçalho de correlação ou protocolo de trace proprietário. Um identificador de correlação adicional só será introduzido se uma integração comprovadamente não propagar o contexto padrão.
- Excluir ou reduzir ruído de endpoints de Health Check e instrumentação interna, sem remover a telemetria das operações de negócio.

O OpenTelemetry utiliza as APIs de logging, métricas e atividades já disponibilizadas pelo .NET e exporta dados por um protocolo independente de fornecedor, permitindo trocar o destino sem acoplar a aplicação a um APM específico.

### 4. Exportação e visualização local

- Utilizar OTLP como protocolo de exportação quando uma ferramenta de visualização estiver ativa.
- Adotar o **Aspire Dashboard em modo standalone** como opção local para visualizar logs estruturados, traces e métricas.
- O dashboard poderá ser iniciado pela Aspire CLI ou por imagem de contêiner no Docker; não exige criar um AppHost nem adotar Aspire para orquestrar o projeto.
- A exportação será configurada por variáveis de ambiente, como `OTEL_EXPORTER_OTLP_ENDPOINT`, mantendo os endereços fora do código.
- Sem endpoint OTLP configurado, a aplicação continua funcionando e mantém logs no console; não deve falhar por ausência do dashboard.
- Manter o dashboard restrito ao ambiente local, em loopback/rede de desenvolvimento, com autenticação por token habilitada. Não usar `--allow-anonymous` em configuração compartilhada ou rede não confiável.
- O dashboard standalone é uma ferramenta de desenvolvimento/diagnóstico de curto prazo: não será tratado como armazenamento de telemetria ou observabilidade de produção. Sua telemetria pode ser limitada e removida quando o processo reinicia.
- Não enviar telemetry com segredos, cookies, identificadores pessoais ou conteúdo financeiro. Os dados fictícios também devem ser minimizados.

Não incluir no MVP Prometheus, Grafana, Jaeger, Zipkin, Azure Monitor ou outro APM como infraestrutura obrigatória. O protocolo OTLP mantém aberta a escolha futura.

### 5. Health Checks e operação

Na API, expor:
- `/health/live`: liveness, informando se o processo está ativo, sem consultar SQL Server ou outras dependências.
- `/health/ready`: readiness, verificando se a API está apta a processar operações que dependem do banco. Incluir uma verificação do `CrediteDbContext` usando `AddDbContextCheck`/EF Core, que por padrão verifica a conectividade com `CanConnectAsync`.
- `/health`: manter como alias compatível para a visão agregada/readiness documentada no início do projeto.

Na aplicação Web, expor `/health/live` para verificar o processo MVC. Não consultar diretamente o banco a partir da Web, nem transformar uma falha momentânea da API em reinício automático do processo MVC. Uma readiness específica do host Web poderá ser acrescentada se o ambiente de implantação exigir avaliação das dependências necessárias para servir determinada rota.

As respostas públicas dos Health Checks devem ser mínimas (status e código HTTP), sem connection strings, detalhes de exceção, nomes de tabelas, servidores ou dados internos. A verificação detalhada fica restrita a logs e diagnóstico local.

A verificação de readiness da API deve ser usada para diagnosticar dependência indisponível; liveness deve representar apenas a saúde do processo. Não configurar repetição automática de requisições do usuário ou reinicializações em cascata por falha temporária do banco.

### 6. Configuração e operação local

- Manter configuração por ambiente em `appsettings.json`, `appsettings.Development.json`, User Secrets e variáveis de ambiente, sem segredos versionados.
- A URL do endpoint OTLP, nível de logs e ativação da exportação serão configuráveis.
- O fluxo local normal continuará sendo executar Web e API pelo Visual Studio Community e SQL Server conforme a configuração de desenvolvimento. O dashboard é opcional.
- Docker Compose poderá iniciar SQL Server e, quando necessário, o Aspire Dashboard standalone; não será obrigatório para executar os hosts .NET.
- Documentar as portas locais, as variáveis OTLP e o procedimento de verificar `/health/live`, `/health/ready`, logs e traces quando a configuração de desenvolvimento for criada.
- Dependências NuGet de instrumentação devem ser estáveis e compatíveis com .NET 10, com versões alinhadas entre si na data da implementação. Não fixar versões de pacotes ou da imagem do dashboard nesta decisão.
- Não adicionar um projeto interno compartilhado só para instrumentação. A configuração de telemetria ficará nos respectivos composition roots/hosts (API e Web), preservando as dependências do ADR-0005.

### 7. Critérios de aceite e testes

- Uma chamada MVC → API deve manter o mesmo trace distribuído, permitindo identificar a chamada HTTP e a atividade correspondente na API.
- Uma falha simulada de SQL Server deve aparecer como falha de readiness da API, sem derrubar o processo nem alterar liveness.
- A Web deve responder a seu endpoint de liveness mesmo quando a API estiver temporariamente indisponível.
- Logs de sucesso e falha devem conter contexto de correlação quando disponível e não podem incluir segredos ou conteúdo sensível.
- As métricas HTTP devem permitir distinguir sucesso e falha sem criar dimensão de alta cardinalidade.
- Com OTLP não configurado, os hosts devem iniciar e operar normalmente; com dashboard local ativo, a telemetria deve ser visualizável.
- Testes automatizados devem verificar o mapeamento HTTP dos Health Checks e a ausência de dados sensíveis nos eventos críticos. A validação do dashboard é uma verificação manual de ambiente, não dependência dos testes unitários.
- Confirmar que endpoints de diagnóstico não exibem detalhes internos para clientes não autorizados.

## Alternativas consideradas

- **Somente logs de console:** simples, mas insuficiente para correlacionar chamadas entre Web, API e SQL ou analisar tendências.
- **Stack fixa Prometheus + Grafana + Jaeger/Zipkin:** oferece visualização madura, mas exige múltiplos serviços, configuração e manutenção desproporcionais ao MVP.
- **Aspire AppHost e ServiceDefaults desde o início:** fornece convenções úteis, mas adicionaria um modelo de orquestração e dependências de service discovery/resiliência que não são necessárias neste monólito local.
- **OpenTelemetry com OTLP e dashboard Aspire standalone opcional — escolhida:** padroniza telemetria e mantém a aplicação independente do destino; permite inspeção local sem impor uma plataforma permanente.
- **Health Checks de terceiros:** foram evitados quando os recursos oficiais ASP.NET Core/EF Core atendem às necessidades atuais.

## Consequências

### Positivas

- Diagnóstico de fluxo completo desde o MVC até API e SQL Server.
- Baixo acoplamento a ferramentas de monitoramento.
- Recursos nativos do .NET permanecem como API de instrumentação.
- Visualização local opcional, sem exigir orquestração Aspire.
- Health Checks distinguem falha do processo de dependência indisponível.

### Negativas / trade-offs

- A exportação OTLP e o dashboard precisam ser iniciados/configurados separadamente para visualização local.
- A configuração de instrumentação deve ser mantida nos dois hosts sem um projeto interno compartilhado.
- Não há retenção durável, alertas ou painéis operacionais de produção no MVP.
- A qualidade dos diagnósticos depende de não registrar dados sensíveis e escolher atributos de baixa cardinalidade.

### Riscos e ações de mitigação

- **Dashboard local expõe telemetria:** manter token ativo, restringir rede e nunca desabilitar autenticação fora de ambiente isolado.
- **Logs vazam dados de negócio:** revisão de templates e propriedades, testes e orientação explícita de não registrar corpos/valores.
- **Métricas com cardinalidade alta:** proibir IDs e texto livre como labels/tags.
- **Health Check provoca reinícios indevidos:** separar liveness de readiness e manter dependências fora do liveness.
- **Instrumentação aumenta custo ou ruído:** filtrar health endpoints, medir overhead e ajustar sampling se necessário.

## Referências

- [Microsoft — Observabilidade do .NET com OpenTelemetry](https://learn.microsoft.com/pt-br/dotnet/core/diagnostics/observability-with-otel)
- [Microsoft — Verificações de integridade no ASP.NET Core (.NET 10)](https://learn.microsoft.com/pt-br/aspnet/core/host-and-deploy/health-checks?view=aspnetcore-10.0)
- [Aspire — Standalone dashboard for OpenTelemetry](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/standalone)
- [Aspire — Dashboard security considerations](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/security-considerations)
- ADR-0003 — Estratégia de testes automatizados.
- ADR-0005 — Arquitetura inicial e limites entre camadas.
- ADR-0006 — Requisitos não funcionais e critérios de qualidade.
- ADR-0010 — Interface administrativa MVC e jornadas.
