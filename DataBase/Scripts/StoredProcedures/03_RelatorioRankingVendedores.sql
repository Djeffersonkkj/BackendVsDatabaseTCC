USE SistemaDeVendas;
GO

/*
    Ranking de vendedores.

    Comentario de desempenho:
    - indices em Pedido(DataPedido) e Pedido(IdVendedor) ajudam o otimizador a reduzir leituras;
    - a ordenacao por SUM(TotalPedido) pode exigir memoria para sort/hash aggregate;
    - a paginacao com OFFSET/FETCH evita retornar mais linhas do que o console precisa exibir.
*/
CREATE OR ALTER PROCEDURE dbo.usp_RelatorioRankingVendedores
    @DataInicio    DATETIME,
    @DataFim       DATETIME,
    @Pagina        INT = 1,
    @TamanhoPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @DataInicio >= @DataFim
        THROW 51102, 'DataInicio deve ser menor que DataFim.', 1;

    SET @Pagina = CASE WHEN @Pagina < 1 THEN 1 ELSE @Pagina END;
    SET @TamanhoPagina = CASE WHEN @TamanhoPagina < 1 THEN 1 ELSE @TamanhoPagina END;

    SELECT
        vendedor.Id AS IdVendedor,
        vendedor.Nome AS Vendedor,
        SUM(pedido.TotalPedido) AS TotalVendido,
        COUNT(*) AS QuantidadePedidos,
        SUM(pedido.ValorComissao) AS ComissaoTotal
    FROM Pedido AS pedido
    INNER JOIN Vendedor AS vendedor
        ON vendedor.Id = pedido.IdVendedor
    WHERE pedido.DataPedido >= @DataInicio
      AND pedido.DataPedido < @DataFim
    GROUP BY
        vendedor.Id,
        vendedor.Nome
    ORDER BY
        TotalVendido DESC,
        Vendedor
    OFFSET (@Pagina - 1) * @TamanhoPagina ROWS
    FETCH NEXT @TamanhoPagina ROWS ONLY;
END;
GO
