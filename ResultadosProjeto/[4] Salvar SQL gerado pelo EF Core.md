# -- SQL gerado por EF Core via ToQueryString()

-- Compare com dbo.usp_RelatorioRankingVendedores.
-- O SQL do EF e derivado de LINQ, parametrizado e sujeito ao tradutor/provider.
-- SQL manual em Stored Procedure permite hints e forma fixa, mas aumenta acoplamento ao SQL Server.

DECLARE @p int = 10;
DECLARE @dataInicio datetime = '2024-01-01T00:00:00.000';
DECLARE @dataFim datetime = '2027-01-01T00:00:00.000';

SELECT TOP(@p) [p].[IdVendedor], [v].[Nome] AS [Vendedor], COALESCE(SUM([p].[TotalPedido]), 0.0) AS [TotalVendido], COUNT(*) AS [QuantidadePedidos], COALESCE(SUM([p].[ValorComissao]), 0.0) AS [ComissaoTotal]
FROM [Pedido] AS [p]
INNER JOIN [Vendedor] AS [v] ON [p].[IdVendedor] = [v].[Id]
WHERE [p].[DataPedido] >= @dataInicio AND [p].[DataPedido] < @dataFim
GROUP BY [p].[IdVendedor], [v].[Nome]
ORDER BY COALESCE(SUM([p].[TotalPedido]), 0.0) DESC, [v].[Nome]
