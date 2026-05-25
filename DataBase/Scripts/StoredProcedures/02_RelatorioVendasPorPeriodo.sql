USE SistemaDeVendas;
GO

/*
    Relatorio de vendas por periodo.

    Equivalente a RelatorioService.ObterVendasPorPeriodoAsync no backend.
    O filtro usa intervalo semiaberto: DataPedido >= @DataInicio e DataPedido < @DataFim.
    Isso evita problemas com horas/minutos/segundos no ultimo dia.
*/
CREATE OR ALTER PROCEDURE dbo.usp_RelatorioVendasPorPeriodo
    @DataInicio DATETIME,
    @DataFim    DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    IF @DataInicio >= @DataFim
        THROW 51101, 'DataInicio deve ser menor que DataFim.', 1;

    SELECT
        YEAR(pedido.DataPedido) AS Ano,
        MONTH(pedido.DataPedido) AS Mes,
        SUM(pedido.TotalPedido) AS TotalVendido,
        COUNT(*) AS QuantidadePedidos,
        AVG(pedido.TotalPedido) AS TicketMedio,
        SUM(pedido.ValorComissao) AS TotalComissao
    FROM Pedido AS pedido
    WHERE pedido.DataPedido >= @DataInicio
      AND pedido.DataPedido < @DataFim
    GROUP BY
        YEAR(pedido.DataPedido),
        MONTH(pedido.DataPedido)
    ORDER BY
        Ano,
        Mes;
END;
GO
