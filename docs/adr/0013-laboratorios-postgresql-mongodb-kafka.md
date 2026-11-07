# ADR-0013 — Laboratórios de PostgreSQL, MongoDB e Apache Kafka

- **Estado:** Aceita para a trilha educacional; não altera a arquitetura do MVP.
- **Data:** 2026-11-07
- **Decisões relacionadas:** ADR-0001 (SQL Server), ADR-0005 (arquitetura), ADR-0012 (evolução distribuída e mensageria).

## Contexto

O foco profissional atual inclui desenvolver competências práticas em PostgreSQL, MongoDB e Apache Kafka, além da stack principal C#/.NET. O Credite já possui SQL Server como banco oficial e monólito modular como arquitetura do MVP.

É necessário adquirir experiência nessas tecnologias sem introduzir infraestrutura desnecessária no produto nem contradizer decisões aprovadas.

## Decisão

Separar claramente tecnologia do produto e tecnologia de laboratório:

- **SQL Server:** banco oficial do Credite e fonte de verdade do MVP, conforme ADR-0001.
- **PostgreSQL:** laboratório relacional comparativo com SQL Server.
- **MongoDB:** laboratório de persistência documental independente.
- **Apache Kafka:** laboratório principal de comunicação assíncrona e streaming de eventos.

Os laboratórios podem reutilizar conceitos e dados inteiramente fictícios do domínio de crédito, mas devem possuir infraestrutura, configuração e ciclo de execução isolados. Não sincronizar bancos entre si nem criar persistência poliglota no Credite sem requisito funcional e ADR específico.

## Escopo mínimo dos laboratórios

### PostgreSQL
- Reproduzir um caso de uso relacional pequeno.
- Comparar tipos, constraints, índices, transações, concorrência e consultas com SQL Server.
- Exercitar EF Core com provider PostgreSQL e documentar diferenças relevantes.

### MongoDB
- Modelar documentos e coleções para um cenário adequado a NoSQL.
- Comparar embedding e referencing.
- Exercitar filtros, índices, atualizações atômicas e validação de documentos.
- Explicar limitações e critérios para não escolher MongoDB.

### Apache Kafka
- Implementar producer e consumer em .NET.
- Exercitar topics, partitions, offsets, consumer groups e chaves.
- Demonstrar duplicidade, idempotência, retry limitado e dead-letter topic.
- Documentar ordenação, retenção, semânticas de entrega e reprocessamento.
- Estudar Outbox somente após compreender transações e o problema de dual-write.

## Alternativas consideradas

- **Substituir SQL Server por PostgreSQL:** rejeitada; contradiz ADR-0001 e não é necessária para aprender PostgreSQL.
- **Adicionar MongoDB ao fluxo principal:** rejeitada; não há caso de uso documental aprovado para o MVP.
- **Adicionar Kafka ao MVP agora:** rejeitada; não existe requisito de mensageria no escopo funcional aprovado.
- **Criar laboratórios isolados:** escolhida; permite aprender, testar falhas e comparar tecnologias sem contaminar a arquitetura do produto.

## Consequências

### Positivas
- Preserva a simplicidade do MVP.
- Permite experiência prática verificável nas tecnologias do foco profissional.
- Facilita comparação entre bancos relacionais e documental.
- Permite estudar mensageria sem impor operação distribuída ao produto.

### Trade-offs
- Os laboratórios exigem manutenção e documentação próprias.
- Conhecimento adquirido em laboratório não deve ser descrito como experiência de produção no Credite.
- Uma futura integração ao produto exigirá novo ADR e justificativa funcional.

## Critérios para considerar o laboratório concluído

Cada laboratório deve possuir:
- README com objetivo, pré-requisitos e execução;
- código C#/.NET funcional;
- testes automatizados relevantes;
- dados sintéticos;
- demonstração de pelo menos uma falha e sua recuperação;
- comparação de alternativas e trade-offs;
- instruções para executar e limpar o ambiente.

## Referências

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [MongoDB Manual](https://www.mongodb.com/docs/manual/)
- [Apache Kafka Documentation](https://kafka.apache.org/documentation/)
- [EF Core Database Providers](https://learn.microsoft.com/ef/core/providers/)
