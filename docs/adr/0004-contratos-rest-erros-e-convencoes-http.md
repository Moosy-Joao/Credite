# ADR-0004 — Contratos REST, erros e convenções HTTP

- **Estado:** Aceita
- **Data:** 2026-11-01
- **Responsáveis:** Projeto Credite

## Contexto

O Credite começa como um monólito modular com uma API ASP.NET Core e uma interface administrativa ASP.NET Core MVC. O MVC consumirá a API por HTTP; não acessará diretamente o banco de dados nem executará regras de negócio pertencentes à API. A API não será pública para terceiros no escopo inicial.

É necessário estabelecer contratos previsíveis para rotas, operações, DTOs, validação, paginação e erros. As convenções devem aproveitar os recursos estáveis do ASP.NET Core 10, facilitar testes automatizados e evitar complexidade desnecessária para um projeto individual.

## Decisão

### Estilo e roteamento

- Utilizar controllers do ASP.NET Core Web API com o atributo `[ApiController]`.
- Prefixar as rotas com `/api`.
- Nomear recursos no plural, em inglês e com rotas orientadas a recursos, por exemplo `/api/customers` e `/api/credit-requests`.
- Utilizar identificadores na rota para operações sobre recursos específicos.
- Utilizar métodos HTTP de acordo com sua semântica: GET para leitura, POST para criação/operações não idempotentes, PUT para substituição/atualização integral e DELETE para remoção quando permitida.
- Evitar verbos arbitrários nas rotas. Ações de domínio que não correspondam a CRUD deverão ter seu contrato explicitamente modelado.

### Contratos e DTOs

- Definir DTOs explícitos para entrada e saída, com nomes como `CreateCustomerRequest` e `CustomerResponse`.
- Não retornar entidades de domínio ou entidades EF Core diretamente pela API.
- Não aceitar entidades de persistência como modelos de entrada.
- Manter ViewModels próprios no projeto MVC; eles não substituem os DTOs da API.
- Tratar alterações incompatíveis nos contratos como decisões explícitas, evitando mudanças silenciosas que quebrem o consumidor MVC.

### Validação

- Utilizar `[ApiController]` e validação de modelos do ASP.NET Core, com Data Annotations quando adequadas para restrições de formato, obrigatoriedade e tamanho.
- Aproveitar a resposta automática HTTP 400 para falhas de validação do modelo.
- Manter regras de negócio e invariantes na Application/Domain; validação HTTP não substitui validação de domínio.
- Não duplicar regras complexas exclusivamente em atributos de DTO.

### Erros e exceções

- Adotar Problem Details conforme RFC 9457, utilizando `AddProblemDetails()` e tratamento centralizado de exceções com `UseExceptionHandler()`.
- Respostas de validação devem seguir o formato `ValidationProblemDetails`.
- Erros de negócio relevantes poderão ter tipos de problema estáveis e identificáveis.
- Não expor stack traces, consultas SQL, segredos, credenciais ou detalhes internos nas respostas.
- Registrar detalhes técnicos nos logs estruturados e correlacioná-los com identificador de rastreamento quando disponível.
- Respostas de endpoints de API não autenticados devem utilizar HTTP 401 e não redirecionar para páginas HTML de login; usuários autenticados sem permissão recebem HTTP 403.
- O consumidor deverá tomar decisões com base no status HTTP e no tipo/código estável do problema, não na interpretação de texto livre de `detail`.

### Códigos HTTP

Adotar, conforme o cenário:

- `200 OK`: leitura ou atualização com representação de resposta.
- `201 Created`: criação bem-sucedida, incluindo cabeçalho `Location` para o recurso criado quando aplicável.
- `204 No Content`: operação concluída sem corpo de resposta.
- `400 Bad Request`: entrada inválida ou falha de validação.
- `401 Unauthorized`: ausência de autenticação válida.
- `403 Forbidden`: identidade autenticada sem autorização.
- `404 Not Found`: recurso inexistente.
- `409 Conflict`: conflito com o estado atual ou regra de negócio.
- `500 Internal Server Error`: falha inesperada, sem detalhes internos na resposta.

O código deve representar a natureza real do resultado; não utilizar 200 para mascarar falhas nem 400 como resposta genérica para todos os erros.

### Paginação e listagens

- Listagens administrativas deverão ser paginadas.
- Utilizar inicialmente paginação por deslocamento com parâmetros `page` e `pageSize`.
- Valores padrão: página 1 e tamanho 20; tamanho máximo 100.
- Respostas paginadas deverão conter `items`, `page`, `pageSize` e `totalCount`, quando a contagem total for necessária ao caso de uso.
- As consultas deverão possuir ordenação determinística, incluindo critério de desempate.
- Não retornar coleções ilimitadas.
- Avaliar paginação por conjunto de chaves (keyset) se volume e padrão de navegação demonstrarem benefício.

### Versionamento

- Não introduzir prefixo de versão, como `/api/v1`, nem biblioteca de versionamento no escopo inicial.
- Mudanças incompatíveis deverão ser avaliadas explicitamente.
- Reavaliar versionamento quando existirem consumidores independentes, integrações externas ou necessidade real de manter contratos incompatíveis em paralelo.

### Idempotência e repetição

- Respeitar a semântica de idempotência definida pelos métodos HTTP.
- Não implementar mecanismo genérico de `Idempotency-Key` nesta fase.
- Não repetir automaticamente operações POST após falhas de comunicação sem determinar se a operação anterior foi efetivada.
- Introduzir proteção específica contra duplicidade quando um fluxo concreto demonstrar essa necessidade.

### Documentação e testes

- Documentar exemplos representativos de request e response dos contratos.
- Testar códigos HTTP, validação, Problem Details, paginação e cenários de autorização nos testes de integração definidos no ADR-0003.
- Manter os contratos coerentes com o uso interno da API pelo MVC.

## Alternativas consideradas

- **Minimal APIs:** são uma opção válida no ASP.NET Core, mas Controllers com `[ApiController]` oferecem uma organização explícita e convenções familiares para os recursos e contratos previstos no projeto.
- **Retornar entidades diretamente:** rejeitado porque expõe detalhes internos de domínio/persistência e acopla o contrato HTTP ao modelo de dados.
- **Erros proprietários sem padrão:** rejeitado porque obrigaria o MVC a interpretar formatos específicos e dificultaria consistência. Problem Details fornece um formato interoperável.
- **Versionamento desde a primeira rota:** adiado porque a API terá um único consumidor controlado no escopo inicial e ainda não há necessidade de manter versões paralelas.
- **Paginação por cursor em todas as listagens:** não adotada inicialmente porque a paginação por deslocamento é mais simples para a interface administrativa e o volume inicial não justifica a complexidade adicional.
- **Idempotência genérica desde o início:** adiada até que exista operação concreta cujo risco de repetição justifique armazenamento e validação de chaves idempotentes.

## Consequências

### Positivas

- Contratos HTTP previsíveis entre MVC e API.
- Separação entre modelos de transporte, domínio, persistência e apresentação.
- Tratamento uniforme de erros e validações.
- Melhor testabilidade dos contratos e da autorização.
- Menor complexidade inicial sem fechar caminhos de evolução.

### Negativas / trade-offs

- DTOs explícitos exigem mapeamento e manutenção.
- Paginação por deslocamento pode perder eficiência em grandes deslocamentos.
- Ausência de versionamento formal exige disciplina ao alterar contratos.
- Problem Details precisa ser configurado e mantido de forma consistente.

### Riscos e ações de mitigação

- **Vazamento de detalhes internos:** centralizar exceções e revisar conteúdo das respostas de erro.
- **Divergência entre MVC e API:** testar contratos HTTP e manter DTOs/documentação atualizados.
- **Paginação instável:** exigir ordenação determinística.
- **Repetição de operações de criação:** evitar retries automáticos de POST e adicionar idempotência específica se necessária.
- **Mudanças incompatíveis:** registrar e revisar alterações de contrato antes de integrá-las.

## Referências

- [Microsoft Learn — Create web APIs with ASP.NET Core](https://learn.microsoft.com/aspnet/core/web-api/)
- [Microsoft Learn — Handle errors in ASP.NET Core APIs](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api)
- [Microsoft Learn — Problem Details in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling)
- [Microsoft Learn — Pagination with EF Core](https://learn.microsoft.com/ef/core/querying/pagination)
- [RFC 9457 — Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc9457)
- [RFC 9110 — HTTP Semantics](https://www.rfc-editor.org/rfc/rfc9110)
