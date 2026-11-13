# Revisão técnica e plano de correções — Credite

- **Base analisada:** `main`
- **Commit-base:** `01f0503f7b8fe1e32214d955dc397a9a06f15011`
- **Branch de trabalho:** `fix/revisao-tecnica-confiabilidade`
- **Escopo:** manter somente Credit API e Notification Worker.
- **Objetivo:** corrigir confiabilidade, validação, consulta e diagnóstico sem adicionar novos microsserviços ou infraestrutura desnecessária.

Este documento é a especificação de implementação desta revisão. As alterações devem ser feitas nesta branch e integradas à `main` somente por Pull Request após validação.

## 1. Cadastro concorrente de clientes — prioridade alta

**Arquivos:** `Endpoints/ClientesEndpoints.cs`, testes e configuração do EF Core.

### Problema
A API consulta se o e-mail existe e depois tenta inserir o cliente. Duas requisições simultâneas podem passar pela consulta; o índice único do PostgreSQL impede a duplicação, mas a segunda operação pode escapar como HTTP 500.

### Correção
Manter a consulta antecipada para resposta rápida e tratar também a violação de unicidade no banco. Identificar especificamente SQLSTATE `23505` e a constraint/índice `ix_clientes_email`; responder HTTP 409 sem mascarar outras falhas de persistência. Não capturar genericamente toda `DbUpdateException`.

### Aceite
- E-mail já cadastrado retorna 409.
- Duas tentativas concorrentes com o mesmo e-mail nunca criam duplicidade.
- Uma falha de banco que não seja conflito continua sendo erro de infraestrutura, não 409.

## 2. Reivindicação concorrente da Outbox — prioridade alta

**Arquivos:** `Dominio/EventoPendente.cs`, `Dados/CrediteDbContext.cs`, migration e snapshot de Credit, `Servicos/PublicadorEventos.cs`.

### Problema
Duas instâncias do Credit podem selecionar o mesmo evento pendente antes de qualquer uma marcá-lo como publicado. A idempotência do consumidor protege o efeito final, mas não evita publicação duplicada nem coordena os publicadores.

### Correção
Reivindicar atomicamente um evento elegível no PostgreSQL usando `SELECT ... FOR UPDATE SKIP LOCKED` dentro de uma transação. O bloqueio da linha impede que outra instância selecione o mesmo evento. Publicar enquanto a transação permanece aberta, com timeout Kafka limitado, então marcar como publicado ou registrar a falha e confirmar a transação. Se o processo cair, o PostgreSQL libera o bloqueio ao encerrar a conexão e o evento permanece pendente para recuperação. Esta é uma escolha proporcional ao MVP; se o volume ou duração de publicação crescer, evoluir para lease persistido e processamento em lotes.

A publicação continua sendo **at-least-once**: se Kafka aceitar a mensagem e o processo cair antes de persistir `publicado_em`, o evento poderá ser publicado novamente. O Notification deve continuar idempotente. Não declarar exactly-once entre PostgreSQL e Kafka.

### Aceite
- Duas instâncias não reivindicam simultaneamente o mesmo evento enquanto o lease estiver válido.
- Queda do publicador libera o bloqueio transacional e permite retomar o evento.
- Falha de publicação incrementa tentativas, registra erro e libera o evento para nova tentativa.
- Publicação bem-sucedida marca `publicado_em` e libera o lease.
- Eventos que atingirem dez tentativas permanecem visíveis para inspeção manual.

## 3. Paginação da listagem — prioridade alta

**Arquivos:** `Endpoints/SolicitacoesEndpoints.cs`, testes, README e contrato da API.

### Problema
A listagem carrega todos os registros. Isso contradiz os requisitos não funcionais registrados no projeto e pode consumir memória e tempo sem limite.

### Correção
Adicionar `pagina` e `tamanhoPagina`, com padrão 1 e 20, mínimo 1 e máximo 100. Aplicar ordenação determinística por `criado_em` e `id`, calcular total de registros e retornar envelope com itens, página, tamanho, total de registros e total de páginas. Preservar filtro opcional por cliente.

### Aceite
- Valores ausentes usam página 1 e tamanho 20.
- Tamanho acima de 100 é rejeitado com erro de validação.
- Página e tamanho inválidos são rejeitados.
- Resultados são estáveis em empates de data.
- Resposta informa metadados de paginação.

## 4. DLQ com metadados — prioridade média

**Arquivo:** `Servicos/ConsumidorEventos.cs` e documentação do contrato.

### Problema
A mensagem inválida é encaminhada à dead-letter topic apenas com chave e payload. Sem origem e posição no Kafka, a investigação fica difícil.

### Correção
Encaminhar payload original e adicionar headers com tópico original, partição, offset, timestamp de falha e motivo resumido. Tratar payload nulo/tombstone e JSON malformado como inválidos. Não registrar conteúdo potencialmente sensível nos logs; o projeto usa dados fictícios, mas logs devem permanecer mínimos.

A confirmação do offset original só ocorre depois de o envio à DLQ ser confirmado. Se o envio à DLQ falhar, manter a mensagem original sem commit e tentar novamente.

### Aceite
- JSON malformado e payload nulo são enviados à DLQ.
- Headers permitem identificar tópico, partição, offset e motivo.
- Falha de publicação na DLQ não confirma o offset original.
- Mensagens válidas continuam seguindo para persistência.

## 5. Idempotência sob concorrência — prioridade média

**Arquivos:** `Servicos/ConsumidorEventos.cs`, persistência e testes.

### Problema
A consulta prévia por `evento_id` não é suficiente para impedir corrida entre consumidores concorrentes. O índice único é a proteção definitiva, mas a violação concorrente pode ser tratada como falha transitória e ficar em retry infinito.

### Correção
Manter o índice único no banco e tratar especificamente a violação de unicidade de `evento_id` como duplicidade já processada. Outras falhas de banco devem continuar gerando retry, sem confirmar o offset.

### Aceite
- Reprocessar o mesmo evento não cria segunda notificação.
- Violação da constraint de idempotência é considerada processamento concluído.
- Erros transitórios de persistência não são confundidos com duplicidade.

## 6. Health check de dependências — prioridade média

**Arquivos:** `Program.cs` da API e documentação.

### Problema
`/health` consulta apenas o PostgreSQL. A API pode responder healthy mesmo quando não consegue alcançar Kafka, que é necessário para completar a publicação assíncrona.

### Correção
Distinguir saúde do processo de prontidão operacional. A rota de readiness deve verificar PostgreSQL e Kafka com timeout limitado, retornando 503 quando uma dependência obrigatória estiver indisponível. Usar cliente Kafka apropriado para consulta de metadados e não deixar a verificação bloquear indefinidamente.

O Worker não expõe HTTP no MVP; seu estado operacional será acompanhado por logs e pelo estado do processo. Não adicionar servidor HTTP apenas para criar um health endpoint.

### Aceite
- PostgreSQL e Kafka disponíveis: readiness 200.
- Qualquer dependência obrigatória indisponível ou timeout: readiness 503.
- A checagem possui timeout limitado e logs úteis.

## 7. Configurações e credenciais locais — prioridade média

**Arquivos:** appsettings, fábricas de design-time, Docker Compose e README.

### Problema
As credenciais atuais são valores fixos de desenvolvimento. Isso é aceitável para ambiente descartável local, mas pode ser confundido com configuração segura para outros ambientes.

### Correção
Manter valores locais claramente identificados como exclusivos de desenvolvimento; permitir sobrescrita por variáveis de ambiente/user secrets nos processos .NET; documentar que não são credenciais de produção. Não introduzir cofre de segredos ou plataforma externa neste MVP. Fábricas de design-time devem respeitar configuração por ambiente quando possível e manter fallback local apenas para geração de migrations.

### Aceite
- README deixa explícito o caráter local das credenciais.
- Configurações podem ser sobrescritas sem editar arquivos versionados.
- Nenhum segredo real é adicionado ao repositório.

## 8. Testes automatizados — prioridade alta

**Arquivos:** projeto `Credite.Tests`, workflow e código testável dos serviços.

### Lacuna atual
Os testes automatizados cobrem essencialmente regras das entidades. O workflow já executa um cenário ponta a ponta feliz com PostgreSQL e Kafka, mas não verifica as principais falhas identificadas.

### Correção
Ampliar testes unitários para limites de paginação, normalização e classificação de falhas. Criar testes de integração executáveis no CI para:
- conflito de e-mail, incluindo corrida concorrente;
- paginação e filtro por cliente;
- reivindicação concorrente da Outbox por duas instâncias e retomada após rollback;
- incremento de tentativas e liberação após falha;
- persistência idempotente de notificações;
- duplicidade por constraint;
- encaminhamento à DLQ com headers;
- falha de envio à DLQ sem commit do offset.

Reutilizar PostgreSQL e Kafka já iniciados no workflow, ou introduzir abstrações pequenas e testáveis quando um teste unitário for mais apropriado. Não adicionar serviços de infraestrutura extras.

### Aceite
Todos os testes passam localmente no CI; o workflow executa restore, build, migrations check, testes e fluxo ponta a ponta.

## 9. Documentação e consistência — prioridade média

**Arquivos:** README, `docs/ARQUITETURA.md`, `docs/CONTRATOS-EVENTOS.md`, `docs/ROADMAP.md`.

### Correção
Atualizar rotas, envelope de paginação, headers da DLQ, semântica at-least-once, bloqueio transacional da Outbox, readiness, credenciais locais e limitações reais. Marcar como concluídas somente funcionalidades cobertas por código e validação. Preservar o escopo de dois microsserviços e não reintroduzir autenticação, UI, gateway, Redis, MongoDB ou serviços extras.

### Aceite
A documentação corresponde ao código e aos testes, sem prometer exactly-once ou alta disponibilidade.

## 10. Ordem de implementação

1. Implementar e testar tratamento de conflito no cadastro.
2. Implementar reivindicação concorrente da Outbox com bloqueio transacional de linha e timeout Kafka limitado.
3. Implementar paginação e testes.
4. Fortalecer idempotência do consumidor.
5. Completar metadados e tratamento de falhas da DLQ.
6. Implementar readiness PostgreSQL + Kafka.
7. Ajustar configuração e credenciais locais.
8. Ampliar testes de integração e cenários de falha no CI.
9. Atualizar documentação e executar validação final.

## 11. Regras de segurança da mudança

- Não alterar diretamente `main`.
- Não reescrever histórico Git.
- Não apagar arquivos ou decisões anteriores sem necessidade.
- Não adicionar dependências ou abstrações sem justificativa.
- Não declarar build/testes aprovados sem execução real.
- Criar commits em português e abrir Pull Request para revisão.
- A validação local no Visual Studio Community continua pendente depois da integração.

## 12. Referência da implementação

Este documento deve ser usado como checklist vinculante durante a implementação. Se a análise do código revelar que algum item não se aplica ou exigir ajuste, registrar a justificativa neste documento e atualizar os critérios de aceite antes de encerrar a tarefa.
