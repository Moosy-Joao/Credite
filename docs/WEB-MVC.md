# Interface Web MVC

O projeto `Credite.Web` fornece uma interface ASP.NET Core MVC para consumir a Credit API. A aplicação Web não acessa PostgreSQL, não publica mensagens no Kafka e não referencia o Notification Worker.

## Responsabilidades

- `Credite.Web`: interface, validação de formulários e comunicação HTTP com a API.
- `Credite.Credit.Api`: regras de negócio, persistência de clientes e solicitações e publicação de eventos.
- `Credite.Notification.Worker`: consumo dos eventos e persistência das notificações.

## Telas

- Início: atalhos para os fluxos disponíveis.
- Clientes: cadastro e consulta por identificador.
- Nova solicitação: criação de solicitação para um cliente existente.
- Solicitações: listagem paginada, filtro opcional por cliente e consulta de detalhes.

A API ainda não possui endpoint para listar todos os clientes. Por isso, o formulário de solicitação recebe o identificador do cliente; a interface não simula uma lista que o Back-End não oferece.

## Configuração

A URL da Credit API é definida em `src/Web/Credite.Web/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "https://localhost:61915/"
  }
}
```

Ajuste o endereço para a URL exibida ao iniciar a Credit API no Visual Studio. Também é possível sobrescrever a configuração com a variável de ambiente `ApiSettings__BaseUrl`.

O perfil de execução do Web usa:

- HTTPS: `https://localhost:7160`
- HTTP: `http://localhost:5160`

## Executar

1. Inicie PostgreSQL e Kafka pelo Docker Compose.
2. Inicie `Credite.Credit.Api` e confirme que a API está disponível.
3. No Visual Studio, defina `Credite.Web` como projeto de inicialização e execute.
4. Abra a URL indicada pelo perfil de execução.

A aplicação Web depende da API estar em execução. Erros HTTP de validação e conflito são apresentados nos formulários.

## Limitações atuais

- Não há autenticação ou autorização.
- Os dados devem ser fictícios. Não utilize dados pessoais ou financeiros reais.
- O status da solicitação é definido pela API; a interface apenas o apresenta.
- O cadastro de clientes não possui listagem geral porque esse endpoint ainda não existe na API.
