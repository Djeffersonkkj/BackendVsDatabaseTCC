# Ordem de criacao das tabelas

As tabelas devem ser criadas respeitando as dependencias de chave estrangeira. Primeiro crie as tabelas que nao dependem de nenhuma outra e, depois, as tabelas que possuem referencias.

## Sequencia recomendada

1. `Endereco.sql`
   - Nao depende de nenhuma tabela.

2. `Cliente.sql`
   - Nao depende de nenhuma tabela.

3. `Situacao.sql`
   - Nao depende de nenhuma tabela.

4. `Categoria.sql`
   - Nao depende de nenhuma tabela.

5. `MetodoPagamento.sql`
   - Nao depende de nenhuma tabela.

6. `Produto.sql`
   - Depende de `Categoria`, por causa de `IdCategoria`.
   - Em bases existentes, execute tambem `Produto_RowVersion_Alter.sql` para adicionar a coluna `RowVersion`.

7. `Vendedor.sql`
   - Depende de `Situacao`, por causa de `IdSituacao`.

8. `Pedido.sql`
   - Depende de `MetodoPagamento`, `Cliente` e `Vendedor`.

9. `PedidoProduto.sql`
   - Depende de `Produto` e `Pedido`.

10. `EnderecoCliente.sql`
    - Depende de `Cliente` e `Endereco`.

## Resumo das dependencias

| Tabela | Depende de |
| --- | --- |
| `Endereco` | Nenhuma |
| `Cliente` | Nenhuma |
| `Situacao` | Nenhuma |
| `Categoria` | Nenhuma |
| `MetodoPagamento` | Nenhuma |
| `Produto` | `Categoria` |
| `Vendedor` | `Situacao` |
| `Pedido` | `MetodoPagamento`, `Cliente`, `Vendedor` |
| `PedidoProduto` | `Produto`, `Pedido` |
| `EnderecoCliente` | `Cliente`, `Endereco` |
