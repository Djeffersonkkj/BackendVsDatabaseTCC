using System.Text;
using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.BusinessRules;
using SistemaDeVendas.Data;
using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interfaces;

namespace SistemaDeVendas.Services;

public sealed class SqlComparacaoService(SistemaDeVendasDbContext context) : ISqlComparacaoService
{
    public async Task<string> SalvarSqlEfCoreAsync(
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        PedidoBusinessRules.ValidarPeriodo(dataInicio, dataFim);

        var consulta = context.Pedidos
            .AsNoTracking()
            .Where(pedido => pedido.DataPedido >= dataInicio && pedido.DataPedido < dataFim)
            .GroupBy(pedido => new { pedido.IdVendedor, pedido.Vendedor.Nome })
            .Select(grupo => new RankingVendedorDto(
                grupo.Key.IdVendedor,
                grupo.Key.Nome,
                grupo.Sum(pedido => pedido.TotalPedido),
                grupo.Count(),
                grupo.Sum(pedido => pedido.ValorComissao)))
            .OrderByDescending(resultado => resultado.TotalVendido)
            .ThenBy(resultado => resultado.Vendedor)
            .Take(10);

        var outputDirectory = Path.Combine(AppContext.BaseDirectory, "SqlCapturado");
        Directory.CreateDirectory(outputDirectory);

        var path = Path.Combine(outputDirectory, $"ef-toquerystring-{DateTime.Now:yyyyMMdd-HHmmss}.sql");
        var builder = new StringBuilder();
        builder.AppendLine("-- SQL gerado por EF Core via ToQueryString().");
        builder.AppendLine("-- Compare com dbo.usp_RelatorioRankingVendedores.");
        builder.AppendLine("-- O SQL do EF e derivado de LINQ, parametrizado e sujeito ao tradutor/provider.");
        builder.AppendLine("-- SQL manual em Stored Procedure permite hints e forma fixa, mas aumenta acoplamento ao SQL Server.");
        builder.AppendLine();
        builder.AppendLine(consulta.ToQueryString());

        await File.WriteAllTextAsync(path, builder.ToString(), cancellationToken);
        return path;
    }
}
