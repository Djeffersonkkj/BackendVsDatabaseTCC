USE SistemaDeVendas;
GO

/*
    Relatorio consolidado.

    Esta e a consulta mais complexa do estudo:
    - junta Pedido, PedidoProduto, Produto, Categoria, Vendedor e MetodoPagamento;
    - agrupa por Categoria + MetodoPagamento + Vendedor;
    - calcula pedidos distintos, quantidade de itens, total vendido, comissao e desconto medio.

    Observacao sobre comissao:
    O backend soma comissao distinta por pedido dentro de cada grupo. A CTE ComissoesPorGrupo
    reproduz essa regra para evitar multiplicar a comissao pelo numero de itens do pedido.

    Comentarios tecnicos:
    - Acoplamento: o formato do relatorio fica fixado no contrato da procedure.
    - Manutencao: mudancas nas colunas exigem atualizar procedure, DTO e leitura ADO.NET.
    - Legibilidade: o CTE deixa a consulta mais explicita, mas ainda e mais densa que LINQ para muitos times.
    - Desempenho: o SQL Server pode otimizar joins/agregacoes diretamente perto dos dados.
    - Custo de processamento: agregacoes, DISTINCT e ORDER BY podem consumir memoria e tempdb em bases grandes.
*/
CREATE OR ALTER PROCEDURE dbo.usp_RelatorioConsolidado
    @DataInicio    DATETIME,
    @DataFim       DATETIME,
    @Pagina        INT = 1,
    @TamanhoPagina INT = 20
AS
BEGIN
    SET NOCOUNT ON;

    IF @DataInicio >= @DataFim
        THROW 51105, 'DataInicio deve ser menor que DataFim.', 1;

    SET @Pagina = CASE WHEN @Pagina < 1 THEN 1 ELSE @Pagina END;
    SET @TamanhoPagina = CASE WHEN @TamanhoPagina < 1 THEN 1 ELSE @TamanhoPagina END;

    WITH Resumo AS
    (
        SELECT
            categoria.Id AS IdCategoria,
            metodoPagamento.Id AS IdMetodoPagamento,
            vendedor.Id AS IdVendedor,
            categoria.Nome AS Categoria,
            metodoPagamento.Nome AS MetodoPagamento,
            vendedor.Nome AS Vendedor,
            COUNT(DISTINCT pedido.Id) AS QuantidadePedidos,
            SUM(CONVERT(INT, item.Quantidade)) AS QuantidadeItensVendidos,
            SUM(item.SubTotal) AS TotalVendido,
            AVG(item.Desconto) AS MediaDesconto
        FROM Pedido AS pedido
        INNER JOIN PedidoProduto AS item
            ON item.IdPedido = pedido.Id
        INNER JOIN Produto AS produto
            ON produto.Id = item.IdProduto
        INNER JOIN Categoria AS categoria
            ON categoria.Id = produto.IdCategoria
        INNER JOIN Vendedor AS vendedor
            ON vendedor.Id = pedido.IdVendedor
        INNER JOIN MetodoPagamento AS metodoPagamento
            ON metodoPagamento.Id = pedido.IdMetodoPagamento
        WHERE pedido.DataPedido >= @DataInicio
          AND pedido.DataPedido < @DataFim
        GROUP BY
            categoria.Id,
            metodoPagamento.Id,
            vendedor.Id,
            categoria.Nome,
            metodoPagamento.Nome,
            vendedor.Nome
        ORDER BY
            TotalVendido DESC,
            categoria.Nome,
            metodoPagamento.Nome,
            vendedor.Nome
        OFFSET (@Pagina - 1) * @TamanhoPagina ROWS
        FETCH NEXT @TamanhoPagina ROWS ONLY
    )
    SELECT
        resumo.Categoria,
        resumo.MetodoPagamento,
        resumo.Vendedor,
        resumo.QuantidadePedidos,
        resumo.QuantidadeItensVendidos,
        resumo.TotalVendido,
        comissao.TotalComissao,
        resumo.MediaDesconto
    FROM Resumo AS resumo
    CROSS APPLY
    (
        SELECT SUM(pedidosDistintos.ValorComissao) AS TotalComissao
        FROM
        (
            SELECT DISTINCT
                pedido.Id,
                pedido.ValorComissao
            FROM Pedido AS pedido
            INNER JOIN PedidoProduto AS item
                ON item.IdPedido = pedido.Id
            INNER JOIN Produto AS produto
                ON produto.Id = item.IdProduto
            WHERE pedido.DataPedido >= @DataInicio
              AND pedido.DataPedido < @DataFim
              AND pedido.IdMetodoPagamento = resumo.IdMetodoPagamento
              AND pedido.IdVendedor = resumo.IdVendedor
              AND produto.IdCategoria = resumo.IdCategoria
        ) AS pedidosDistintos
    ) AS comissao
    ORDER BY
        resumo.TotalVendido DESC,
        resumo.Categoria,
        resumo.MetodoPagamento,
        resumo.Vendedor;
END;
GO
