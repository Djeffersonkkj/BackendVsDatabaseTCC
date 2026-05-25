# Relatorio tecnico - comparacao Backend x Stored Procedures

## Alteracoes realizadas

- Centralizacao das regras de pedido em `PedidoBusinessRules`.
- Backend passou a validar produto duplicado, periodo invalido e mensagens equivalentes.
- Calculos de subtotal e comissao usam a mesma formula e arredondamento de 2 casas.
- `Produto` recebeu `RowVersion` com `[Timestamp]` e mapeamento EF Core como concurrency token.
- `PedidoService` passou a tratar `DbUpdateConcurrencyException` para impedir baixa de estoque inconsistente.
- Benchmark saiu de `Program.cs` e foi movido para `BenchmarkService`.
- Benchmark agora usa cenario fixo: cliente, vendedor, metodo, data, produtos, quantidades e descontos deterministas.
- Benchmark executa warm-up, multiplas execucoes, media, mediana, minimo, maximo e desvio padrao.
- Cenario e restaurado entre execucoes removendo pedidos criados e retornando estoque aos valores originais.
- Adicionado interceptor para capturar SQL do EF Core, parametros e duracao das queries em arquivo.
- Adicionado uso de `ToQueryString()` para salvar SQL LINQ comparavel com Stored Procedure.
- Script de indices foi reorganizado com justificativa tecnica e impacto em escrita.

## Problemas resolvidos

- Produto duplicado era recusado na procedure e aceito no backend.
- Periodo invalido era recusado nas procedures de relatorio e aceito no LINQ.
- Mensagem de estoque insuficiente no backend variava por produto; agora e equivalente a procedure.
- `DateTime.Now` e consumo acumulado de estoque deixavam o benchmark nao reproduzivel.
- `Program.cs` concentrava menu, benchmark, regras de exibicao e comparacao.
- Medicao unica foi substituida por amostra com estatistica basica.

## Problemas restantes e limitacoes

- O backend usa concorrencia otimista com `RowVersion`; a procedure usa `UPDLOCK`. Sao equivalentes quanto a impedir venda inconsistente, mas nao identicos quanto a bloqueio, espera e retry.
- O reset do benchmark restaura dados funcionais, mas nao zera identity, cache de dados, cache de planos, estatisticas internas do SQL Server ou efeitos externos de concorrencia.
- A separacao de tempo de procedure entre processamento e persistencia e limitada, porque a Stored Procedure executa regra e persistencia dentro de uma chamada unica ao SQL Server.
- A captura de SQL do EF mostra o comando gerado/executado, mas planos de execucao ainda precisam ser analisados no SQL Server para conclusao final de desempenho.

## Impacto arquitetural

- A comparacao ficou mais valida sem criar camadas enterprise.
- `Program.cs` virou apenas orquestrador de menu.
- Regras compartilhadas ficaram explicitas em uma classe pequena e testavel.
- Stored Procedures continuam como contrato SQL separado, com equivalencia documentada no C#.

## Impacto em desempenho

- `RowVersion` adiciona custo pequeno em updates de `Produto`, compensado por seguranca concorrente.
- Indices aceleram relatorios por periodo, joins e agregacoes, mas aumentam custo de `INSERT` em `Pedido`/`PedidoProduto`.
- Warm-up reduz ruido inicial de conexao, JIT, cache de modelo EF e plano de execucao.

## Impacto em manutenibilidade

- Novas regras de pedido devem ser alteradas primeiro em `PedidoBusinessRules` e refletidas nas procedures.
- Benchmark ficou isolado e pode evoluir sem poluir o console.
- Scripts SQL documentam motivos e custos, melhorando rastreabilidade academica.

## Impacto em concorrencia

- Backend nao depende mais de leitura simples seguida de update vulneravel.
- Se dois pedidos tentarem baixar o mesmo estoque simultaneamente, o EF valida `RowVersion` no `UPDATE`.
- Em conflito, o pedido falha com mensagem controlada em vez de vender estoque desatualizado.

## Prontidao para fase final

O projeto esta tecnicamente mais pronto para a fase final de comparacao. A comparacao agora e funcionalmente equivalente nas regras centrais, reproduzivel dentro da base atual e estatisticamente mais confiavel. Para conclusao final do TCC, recomenda-se registrar ambiente, volume de dados, indices aplicados, configuracao do SQL Server e planos de execucao das consultas principais.
