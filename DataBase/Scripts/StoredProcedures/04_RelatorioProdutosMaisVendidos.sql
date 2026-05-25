USE SistemaDeVendas;
GO

/*
    Produtos mais vendidos.

    Regra:
    - filtra pedidos pelo periodo;
    - agrupa por produto e categoria;
    - soma quantidade e faturamento;
    - calcula desconto medio.

    Esta consulta costuma ser mais pesada que rankings simples porque PedidoProduto tende a crescer rapido.
*/
CREATE OR ALTER PROCEDURE dbo.usp_RelatorioProdutosMaisVendidos
    @DataInicio    DATETIME,
    @DataFim       DATETIME,
    @Pagina        INT = 1,
    @TamanhoPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @DataInicio >= @DataFim
        THROW 51103, 'DataInicio deve ser menor que DataFim.', 1;

    SET @Pagina = CASE WHEN @Pagina < 1 THEN 1 ELSE @Pagina END;
    SET @TamanhoPagina = CASE WHEN @TamanhoPagina < 1 THEN 1 ELSE @TamanhoPagina END;

    SELECT
        produto.Id AS IdProduto,
        produto.Nome AS Produto,
        categoria.Nome AS Categoria,
        SUM(CONVERT(INT, item.Quantidade)) AS QuantidadeVendida,
        SUM(item.SubTotal) AS Faturamento,
        AVG(item.Desconto) AS MediaDesconto
    FROM PedidoProduto AS item
    INNER JOIN Pedido AS pedido
        ON pedido.Id = item.IdPedido
    INNER JOIN Produto AS produto
        ON produto.Id = item.IdProduto
    INNER JOIN Categoria AS categoria
        ON categoria.Id = produto.IdCategoria
    WHERE pedido.DataPedido >= @DataInicio
      AND pedido.DataPedido < @DataFim
    GROUP BY
        produto.Id,
        produto.Nome,
        categoria.Nome
    ORDER BY
        QuantidadeVendida DESC,
        Faturamento DESC
    OFFSET (@Pagina - 1) * @TamanhoPagina ROWS
    FETCH NEXT @TamanhoPagina ROWS ONLY;
END;
GO
