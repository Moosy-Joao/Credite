# Requisitos funcionais e escopo do MVP

- **Estado:** Aprovado
- **Data:** 2026-11-01
- **Base de produto:** ADR-0007 — Visão do produto, atores e limites do Credite
- **Decisões relacionadas:** ADR-0002 (autenticação e autorização), ADR-0003 (testes), ADR-0004 (contratos REST), ADR-0005 (arquitetura e camadas), ADR-0006 (requisitos não funcionais), ADR-0008 (domínio), ADR-0009 (simulação demonstrativa)

## 1. Objetivo

Definir o primeiro incremento utilizável do Credite: uma aplicação administrativa que permita percorrer um fluxo completo de gestão de solicitações de crédito fictícias, desde o cadastro do cliente até a avaliação demonstrativa e a consulta do histórico.

O MVP deve demonstrar um fluxo vertical coerente, com persistência, regras de negócio explícitas, autorização no servidor, interface administrativa e testes automatizados. A prioridade é entregar comportamento verificável, não maximizar a quantidade de tecnologias ou serviços.

Este documento detalha o escopo funcional aprovado. Não substitui os ADRs técnicos nem antecipa decisões de modelagem que pertencem à fase de domínio.

## 2. Princípios e limites

1. Todos os clientes, solicitações, valores e resultados são fictícios e sintéticos.
2. O sistema não realiza análise real de crédito, não calcula score financeiro real e não aprova ou nega crédito de verdade.
3. Não inserir CPF, documentos, credenciais, dados bancários ou informações financeiras reais.
4. O cliente fictício é um registro de negócio, não uma conta de autenticação.
5. A autorização é aplicada e validada pela API. Ocultar ações na interface não é controle de segurança.
6. A interface MVC consome a API por HTTP e não acessa diretamente o banco nem implementa regras de negócio.
7. O MVP é implementado inicialmente no monólito modular definido no ADR-0005.
8. Os estados, transições, invariantes e cálculos detalhados serão definidos nos itens próprios da fase de domínio.

## 3. Atores

| Ator | Responsabilidade no MVP |
|---|---|
| Administrador | Gerenciar contas internas e perfis; também pode consultar e operar os registros conforme a matriz de permissões |
| Operador | Cadastrar e manter clientes fictícios, registrar solicitações e acompanhar seu andamento |
| Analista | Consultar clientes e solicitações, executar simulações e registrar avaliações demonstrativas |

Não existem cadastro público, login de clientes ou portal externo no MVP. Recrutadores e visitantes do portfólio são stakeholders, não usuários operacionais.

## 4. Requisitos funcionais

### 4.1 Autenticação e administração de contas

- **RF-001 — Autenticar usuário:** o sistema deve permitir que contas internas ativas entrem com suas credenciais.
- **RF-002 — Encerrar sessão:** o usuário autenticado deve poder sair do sistema.
- **RF-003 — Listar contas:** o Administrador deve consultar as contas internas existentes.
- **RF-004 — Criar conta interna:** o Administrador deve criar uma conta e atribuir um dos perfis definidos no ADR-0002.
- **RF-005 — Alterar perfil:** o Administrador deve poder alterar o perfil de uma conta, respeitando as regras de segurança.
- **RF-006 — Desativar conta:** o Administrador deve poder impedir que uma conta desativada continue autenticando-se.
- **RF-007 — Restringir administração:** operações de gestão de contas devem ser negadas a Operadores e Analistas.

Não fazem parte do MVP recuperação de senha por e-mail, autenticação social, cadastro público ou federação de identidade.

### 4.2 Gestão de clientes fictícios

- **RF-008 — Cadastrar cliente:** Operador ou Administrador deve cadastrar um cliente com dados sintéticos.
- **RF-009 — Listar clientes:** usuários autorizados devem consultar clientes com paginação.
- **RF-010 — Consultar cliente:** usuários autorizados devem visualizar os dados de um cliente.
- **RF-011 — Editar cliente:** Operador ou Administrador deve alterar os dados permitidos de um cliente.
- **RF-012 — Consultar solicitações do cliente:** usuários autorizados devem localizar as solicitações associadas a um cliente.

Os atributos obrigatórios, validações, unicidade e política de exclusão serão definidos na modelagem do domínio. Não presumir CPF como identificador.

### 4.3 Gestão de solicitações

- **RF-013 — Registrar solicitação:** Operador ou Administrador deve criar uma solicitação vinculada a um cliente fictício existente.
- **RF-014 — Listar solicitações:** usuários autorizados devem consultar solicitações com paginação e filtros relevantes.
- **RF-015 — Consultar solicitação:** usuários autorizados devem visualizar os detalhes de uma solicitação.
- **RF-016 — Acompanhar estado:** usuários autorizados devem visualizar o estado atual da solicitação.
- **RF-017 — Alterar estado:** somente atores autorizados podem executar transições permitidas pelas regras de domínio.
- **RF-018 — Consultar histórico:** usuários autorizados devem visualizar a sequência de eventos relevantes da solicitação.

A lista definitiva de estados, transições, pré-condições e ações permitidas será definida em decisão de domínio específica. O MVP não deve permitir alteração arbitrária de estado fora dessas regras.

### 4.4 Simulação e avaliação demonstrativa

- **RF-019 — Consultar fila de análise:** Analista ou Administrador deve localizar solicitações elegíveis para avaliação.
- **RF-020 — Executar simulação:** Analista ou Administrador deve executar uma simulação com parâmetros fictícios e regras determinísticas documentadas.
- **RF-021 — Exibir explicação:** o resultado deve apresentar os parâmetros e as regras demonstrativas utilizados, de modo que seja possível compreender como o resultado foi produzido.
- **RF-022 — Registrar avaliação:** o sistema deve associar à solicitação o resultado demonstrativo, a versão ou identificação das regras aplicadas, os parâmetros relevantes, o autor e a data/hora.
- **RF-023 — Consultar avaliações:** usuários autorizados devem consultar avaliações já registradas, sem sobrescrever silenciosamente resultados anteriores.

O método aprovado é Tabela Price demonstrativa, com taxa mensal fixa, principal de R$ 100,00 a R$ 100.000,00, taxa de 0,00% a 5,00% a.m. e prazo de 1 a 60 meses. A regra de arredondamento, cronograma, exemplo de referência, snapshot/versionamento e testes estão definidos no ADR-0009. O registro de avaliação preserva o cálculo; não produz score, classificação de risco, recomendação, aprovação ou negativa financeira real.

### 4.5 Histórico e rastreabilidade

- **RF-024 — Registrar eventos relevantes:** o sistema deve registrar criação de solicitação, mudanças de estado e execução/registro de avaliação.
- **RF-025 — Identificar autoria e momento:** cada evento registrado deve identificar o usuário responsável e a data/hora.
- **RF-026 — Preservar sequência:** a consulta do histórico deve apresentar os eventos em ordem determinística e permitir reconstruir as principais etapas do fluxo.

O histórico do MVP é voltado à rastreabilidade funcional da solicitação. Não representa, por si só, um sistema corporativo completo de auditoria ou imutabilidade.

## 5. Matriz inicial de permissões

A tabela define o comportamento funcional esperado. A implementação deverá usar políticas/roles no servidor, conforme ADR-0002, e deverá ser verificada por testes de integração.

| Operação | Operador | Analista | Administrador |
|---|:---:|:---:|:---:|
| Entrar e sair | Sim | Sim | Sim |
| Consultar contas internas | Não | Não | Sim |
| Criar, alterar perfil e desativar contas | Não | Não | Sim |
| Consultar clientes | Sim | Sim | Sim |
| Cadastrar e editar clientes | Sim | Não | Sim |
| Criar solicitações | Sim | Não | Sim |
| Consultar solicitações e histórico | Sim | Sim | Sim |
| Executar simulações | Não | Sim | Sim |
| Registrar avaliações | Não | Sim | Sim |

A matriz é uma definição funcional inicial. A especificação de políticas nomeadas e eventuais restrições por recurso deve permanecer coerente com o ADR-0002. A API é a autoridade para permitir ou negar cada operação.

## 6. Fluxo vertical principal de aceite

O MVP deverá permitir demonstrar o seguinte cenário de ponta a ponta:

1. O Administrador autentica-se e cria contas para um Operador e um Analista.
2. O Operador autentica-se e cadastra um cliente com dados inteiramente fictícios.
3. O Operador cria uma solicitação vinculada ao cliente.
4. A solicitação aparece na consulta e pode ser aberta para visualização.
5. O Analista autentica-se e localiza uma solicitação elegível.
6. O Analista executa uma simulação demonstrativa.
7. O sistema apresenta o resultado, os parâmetros e a explicação das regras utilizadas.
8. O resultado da avaliação é registrado e associado à solicitação.
9. O histórico apresenta as principais operações, seus autores e momentos.
10. Tentativas de executar operações sem permissão são negadas pela API.

## 7. Critérios de aceite transversais

- Entradas inválidas são rejeitadas na fronteira HTTP com os erros definidos no ADR-0004.
- Recursos inexistentes produzem respostas coerentes, sem expor detalhes internos.
- Regras e transições inválidas são rejeitadas pela camada de domínio/aplicação, não apenas pela interface.
- Usuários não autorizados recebem 401 quando não autenticados e 403 quando autenticados sem permissão, conforme ADR-0004.
- Listagens respeitam paginação e limites definidos no ADR-0004.
- Dados persistidos permanecem disponíveis após reiniciar a aplicação.
- O histórico identifica autor e data/hora para os eventos definidos.
- Resultados de simulação podem ser explicados a partir dos parâmetros e regras registrados.
- Testes automatizados cobrem o fluxo principal, validações, regras, persistência e autorização, conforme ADR-0003.
- Todas as massas de teste e demonstrações usam dados sintéticos.

## 8. Fora do escopo do MVP

- RabbitMQ, MassTransit e processamento assíncrono por broker.
- YARP como API Gateway.
- Redis ou outra camada de cache distribuído.
- Extração de microserviços ou implantação distribuída.
- Outbox, Saga, DLQ e Circuit Breaker.
- Notificações por e-mail, SMS, WhatsApp ou push.
- Integração com bancos, birôs de crédito ou serviços financeiros.
- Cadastro público, login de clientes e portal do cliente.
- Operações financeiras reais, contratação, liberação de valores ou pagamentos.
- Score financeiro real, análise real de capacidade de pagamento ou decisão de crédito.
- Dashboards analíticos avançados, relatórios gerenciais e exportações.
- Aplicativo mobile e testes E2E de navegador nesta etapa.

Esses itens podem ser reavaliados em fases futuras, mediante necessidade demonstrável e decisão explícita. A arquitetura distribuída não é requisito para considerar o MVP concluído.

## 9. Organização de entrega

A entrega deve ocorrer em incrementos pequenos e integrados, mantendo o fluxo vertical verificável. A ordem técnica detalhada será refinada após a definição do domínio e dos contratos:

1. Preparar autenticação, autorização e estrutura de persistência.
2. Implementar clientes fictícios e seus testes.
3. Implementar solicitações, estados e histórico.
4. Implementar regras de simulação e registro das avaliações.
5. Integrar a interface MVC por HTTP.
6. Completar testes de integração e critérios de aceite do fluxo ponta a ponta.

Esta sequência é uma orientação de decomposição, não uma autorização para iniciar implementação. A Fase 0 do roadmap continua obrigatória: clonar o repositório, abrir a solução no Visual Studio Community, confirmar o SDK .NET 10, executar restore/build/test, registrar os resultados e confirmar explicitamente a conclusão.

## 10. Decisões ainda necessárias

Este documento não fecha decisões que pertencem aos próximos itens de planejamento:

- Vocabulário, entidades, value objects e agregados do domínio.
- Estados e transições permitidas de uma solicitação.
- Campos e validações dos clientes e solicitações.
- Política de histórico/auditoria e retenção.
- Telas, navegação, ViewModels e acessibilidade da interface.
- Estratégia de configuração e ambientes.

## 11. Referências internas

- ADR-0002 — Autenticação, autorização e dados fictícios.
- ADR-0003 — Estratégia de testes automatizados.
- ADR-0004 — Contratos REST, erros e convenções HTTP.
- ADR-0005 — Arquitetura inicial e limites entre camadas.
- ADR-0006 — Requisitos não funcionais e critérios de qualidade.
- ADR-0007 — Visão do produto, atores e limites do Credite.
- ADR-0008 — Modelagem do domínio e ciclo de vida da solicitação.
- ADR-0009 — Regras demonstrativas de simulação e avaliação.
- [Roadmap do Credite](ROADMAP.md)
