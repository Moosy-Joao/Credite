# Credite

Projeto educacional de Back-End com C# 14, .NET 10 e dois microsserviços. O sistema registra solicitações fictícias de crédito e processa notificações de forma assíncrona.

O Credite não realiza análise financeira real, não concede crédito e não deve receber dados pessoais ou financeiros reais.

## Arquitetura

| Microsserviço | Responsabilidade | Banco |
|---|---|---|
| Credit API | Cadastro de clientes fictícios, solicitações e publicação de eventos | PostgreSQL credito |
| Notification Worker | Consumo de eventos e registro idempotente de notificações | PostgreSQL notificacoes |

O Credit grava a solicitação e um evento pendente na mesma transação. Um processo de Outbox publica os eventos no Apache Kafka. O Notification consome os eventos, registra a notificação e confirma o offset somente após persistir os dados.

Cada serviço é dono do próprio banco. Os bancos compartilham apenas a instância PostgreSQL no ambiente local, não tabelas ou migrations.

## Tecnologias

- C# 14 e .NET 10
- ASP.NET Core Minimal APIs
- EF Core 10 e PostgreSQL
- Apache Kafka e Confluent.Kafka
- Worker Service
- OpenAPI e Scalar
- xUnit
- Docker Compose
- GitHub Actions

## Estrutura

~~~text
Credite/
├── src/
│   ├── Services/
│   │   ├── Credit/Credite.Credit.Api/
│   │   └── Notification/Credite.Notification.Worker/
│   └── Tests/Credite.Tests/
├── infra/postgres/init.sql
├── docs/
├── docker-compose.yml
└── Credite.slnx
~~~

Cada microsserviço possui um único projeto executável. As pastas internas separam responsabilidades sem criar projetos de camadas prematuramente.

## Pré-requisitos

- .NET 10 SDK
- Docker Desktop com Docker Compose
- Visual Studio Community com suporte a .NET

## Executar localmente

Na raiz do repositório, suba a infraestrutura:

~~~powershell
docker compose up -d
docker compose ps
~~~

Aguarde o PostgreSQL e o Kafka ficarem disponíveis. Na primeira inicialização, o PostgreSQL cria os bancos credito e notificacoes.

Restaure as dependências, as ferramentas locais, compile e teste:

~~~powershell
dotnet restore Credite.slnx
dotnet tool restore
dotnet build Credite.slnx
dotnet test Credite.slnx
~~~

Inicie a API em um terminal:

~~~powershell
dotnet run --project src/Services/Credit/Credite.Credit.Api
~~~

A API aplica suas migrations automaticamente na inicialização. A documentação interativa fica em /scalar/v1, na URL indicada pelo terminal.

Inicie o Worker em outro terminal:

~~~powershell
dotnet run --project src/Services/Notification/Credite.Notification.Worker
~~~

O Worker também aplica suas migrations automaticamente e começa a consumir o tópico Kafka. Os dois serviços carregam as configurações locais a partir dos arquivos appsettings distribuídos junto aos executáveis, independentemente da pasta atual do terminal.

Para encerrar a infraestrutura:

~~~powershell
docker compose down
~~~

Para apagar também os dados locais do PostgreSQL:

~~~powershell
docker compose down -v
~~~

O segundo comando remove permanentemente os dados armazenados no volume local.

## Fluxo de uso

### 1. Cadastrar cliente

~~~http
POST /clientes
Content-Type: application/json

{
  "nome": "Ana Silva",
  "email": "ana@exemplo.com"
}
~~~

### 2. Criar solicitação de crédito

Use o id retornado no cadastro:

~~~http
POST /solicitacoes
Content-Type: application/json

{
  "clienteId": "COLOQUE-O-ID-DO-CLIENTE",
  "valorSolicitado": 2500.00
}
~~~

O valor permitido é de R$ 100,00 a R$ 10.000,00. A solicitação inicia com status EmAnalise.

### 3. Consultar solicitação

~~~http
GET /solicitacoes/ID-DA-SOLICITACAO
~~~

Também é possível listar solicitações de um cliente:

~~~http
GET /solicitacoes?clienteId=ID-DO-CLIENTE&pagina=1&tamanhoPagina=20
~~~

### 4. Verificar notificação

Após o evento ser publicado e consumido, a notificação fica registrada no banco notificacoes, tabela notificacao.notificacoes. O Worker informa o processamento no log.

## Endpoints

| Método | Rota | Descrição |
|---|---|---|
| GET | /health/live | Verifica se o processo da API está ativo |\n| GET | /health | Verifica prontidão de PostgreSQL e Kafka |
| POST | /clientes | Cadastra cliente fictício |
| GET | /clientes/{id} | Consulta cliente |
| POST | /solicitacoes | Cria solicitação e evento Outbox |
| GET | /solicitacoes | Lista solicitações, opcionalmente filtradas por cliente |
| GET | /solicitacoes/{id} | Consulta solicitação |

## Configuração e credenciais\n\nAs credenciais versionadas em `appsettings.json`, nas fábricas de design-time e no Docker Compose são **exclusivas para desenvolvimento local descartável**. Não reutilize esses valores em produção. Os processos .NET aceitam sobrescrita pela variável de ambiente `ConnectionStrings__BancoDados`; o EF Core design-time também respeita essa variável. Não armazene credenciais reais no repositório.\n\n## Migrations

As migrations são independentes por microsserviço. Cada projeto possui uma fábrica de design-time para que o EF Core consiga criar o DbContext sem iniciar o serviço.

Verifique se os snapshots correspondem aos modelos:

~~~powershell
dotnet ef migrations has-pending-model-changes --project src/Services/Credit/Credite.Credit.Api/Credite.Credit.Api.csproj
dotnet ef migrations has-pending-model-changes --project src/Services/Notification/Credite.Notification.Worker/Credite.Notification.Worker.csproj
~~~

Para criar uma migration no Credit:

~~~powershell
dotnet ef migrations add NomeDaMigracao --project src/Services/Credit/Credite.Credit.Api/Credite.Credit.Api.csproj --output-dir Dados
~~~

Para criar uma migration no Notification:

~~~powershell
dotnet ef migrations add NomeDaMigracao --project src/Services/Notification/Credite.Notification.Worker/Credite.Notification.Worker.csproj --output-dir Dados
~~~

Revise sempre o código gerado antes de aplicar ou enviar uma migration.

## Mensageria e consistência

- Tópico principal: solicitacoes.credito.criada.
- Dead-letter topic: solicitacoes.credito.criada.dlq.
- A Outbox evita perder o evento entre a gravação da solicitação e a publicação no Kafka. O publicador reivindica registros com bloqueio transacional de linha e `FOR UPDATE SKIP LOCKED`, evitando que duas instâncias processem simultaneamente o mesmo registro. A entrega é at-least-once: pode haver republicação se o processo cair depois do envio ao Kafka e antes de salvar a confirmação. O consumidor é idempotente.
- O Notification usa eventoId único para impedir notificações duplicadas.
- O offset só é confirmado após persistência ou envio bem-sucedido para a DLQ.

Consulte docs/CONTRATOS-EVENTOS.md para o payload.

## Convenções

- Solução, projetos, namespaces e pastas em inglês.
- Identificadores internos em C# em português, seguindo as convenções oficiais.
- Tabelas e colunas em português com snake_case.
- Documentação e commits em português.
- Preferir recursos nativos do .NET e evitar abstrações prematuras.

## Limitações do MVP

- Não há autenticação, autorização ou interface Web.
- A análise de crédito é fictícia; a API apenas registra a solicitação como EmAnalise.
- A Outbox limita a dez tentativas e exige inspeção manual após esgotá-las.
- O Worker repete falhas temporárias de persistência até conseguir processar a mensagem.
- A DLQ não possui ferramenta de replay automático. As mensagens inválidas incluem headers com tópico, partição, offset, horário e motivo da falha.
- A configuração do Docker Compose é exclusivamente para desenvolvimento local.

## Documentação

- docs/ARQUITETURA.md
- docs/ROADMAP.md
- docs/CONTRATOS-EVENTOS.md
- docs/adr/0001-dois-microsservicos.md
