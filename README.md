# Credite

Projeto educacional de Back-End com C# 14, .NET 10 e dois microsserviços. O sistema simula o registro de solicitações de crédito e o processamento assíncrono de notificações.

O Credite usa somente dados fictícios. Não realiza análise financeira real, não concede crédito e não deve receber dados pessoais ou financeiros reais.

## Objetivo

Aprender a construir e integrar dois microsserviços independentes, mantendo uma aplicação pequena o suficiente para ser concluída e compreendida.

## Arquitetura

| Microsserviço | Responsabilidade | Persistência |
|---|---|---|
| Credit API | Cadastrar clientes fictícios e gerenciar solicitações de crédito | PostgreSQL próprio |
| Notification Worker | Consumir eventos do Credit e registrar notificações | PostgreSQL próprio |

O Credit publica eventos no Apache Kafka. O Notification consome esses eventos sem depender diretamente do código ou do banco do Credit.

Cada serviço é dono dos próprios dados. Os dois bancos podem usar a mesma instância local do PostgreSQL, mas serão bancos separados.

## Tecnologias

- C# 14 e .NET 10
- ASP.NET Core Minimal APIs
- PostgreSQL e Entity Framework Core
- Apache Kafka
- Worker Service
- xUnit
- Docker Compose

As dependências serão adicionadas quando cada etapa precisar delas. Não manteremos pacotes ou infraestrutura sem uso.

## Estrutura

```text
Credite/
├── src/
│   └── Services/
│       ├── Credit/
│       │   └── Credite.Credit.Api/
│       └── Notification/
│           └── Credite.Notification.Worker/
├── docs/
│   ├── ARQUITETURA.md
│   ├── ROADMAP.md
│   └── adr/
│       └── 0001-dois-microsservicos.md
├── Credite.slnx
├── Directory.Build.props
└── Directory.Packages.props
```

Cada microsserviço começa com um único projeto executável. Pastas e camadas adicionais só serão criadas quando houver necessidade concreta.

## Executar

Requisito: .NET 10 SDK.

```powershell
dotnet restore Credite.slnx
dotnet build Credite.slnx
```

Para iniciar a API:

```powershell
dotnet run --project src/Services/Credit/Credite.Credit.Api
```

A API disponibiliza `GET /health` para verificar se o processo está ativo.

Para iniciar o Worker:

```powershell
dotnet run --project src/Services/Notification/Credite.Notification.Worker
```

A integração com PostgreSQL e Kafka será implementada nas etapas do roadmap. Até lá, os projetos são somente esqueletos executáveis.

## Convenções

- Solução, projetos, namespaces e pastas em inglês.
- Identificadores escritos pelo projeto em C# em português, seguindo as convenções oficiais.
- Tabelas e colunas em português com `snake_case`.
- Documentação e mensagens de commit em português.
- Preferir recursos nativos do .NET e evitar abstrações prematuras.

## Escopo excluído

Não fazem parte do projeto atual: Identity/autenticação, API Gateway, interface Web, Redis, MongoDB, múltiplos serviços de domínio, Clean Architecture com projetos separados por camada, Saga e observabilidade distribuída avançada.

Esses itens não serão adicionados sem uma necessidade de aprendizado claramente definida.

## Documentação

- [Arquitetura](docs/ARQUITETURA.md)
- [Roadmap](docs/ROADMAP.md)
- [ADR-0001 — Dois microsserviços](docs/adr/0001-dois-microsservicos.md)
