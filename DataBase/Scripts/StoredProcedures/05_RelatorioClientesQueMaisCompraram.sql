USE SistemaDeVendas;
GO

/*
    Clientes que mais compraram.

    A procedure reproduz a projection do backend:
    - total gasto por cliente;
    - quantidade de pedidos;
    - ticket medio.
*/
CREATE OR ALTER PROCEDURE dbo.usp_RelatorioClientesQueMaisCompraram
    @DataInicio    DATETIME,
    @DataFim       DATETIME,
    @Pagina        INT = 1,
    @TamanhoPagina INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    IF @DataInicio >= @DataFim
        THROW 51104, 'DataInicio deve ser menor que DataFim.', 1;

    SET @Pagina = CASE WHEN @Pagina < 1 THEN 1 ELSE @Pagina END;
    SET @TamanhoPagina = CASE WHEN @TamanhoPagina < 1 THEN 1 ELSE @TamanhoPagina END;

    SELECT
        cliente.Id AS IdCliente,
        cliente.Nome AS Cliente,
        SUM(pedido.TotalPedido) AS TotalGasto,
        COUNT(*) AS QuantidadePedidos,
        AVG(pedido.TotalPedido) AS TicketMedio
    FROM Pedido AS pedido
    INNER JOIN Cliente AS cliente
        ON cliente.Id = pedido.IdCliente
    WHERE pedido.DataPedido >= @DataInicio
      AND pedido.DataPedido < @DataFim
    GROUP BY
        cliente.Id,
        cliente.Nome
    ORDER BY
        TotalGasto DESC,
        Cliente
    OFFSET (@Pagina - 1) * @TamanhoPagina ROWS
    FETCH NEXT @TamanhoPagina ROWS ONLY;
END;
GO
