using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.BusinessRules;
using SistemaDeVendas.Data;
using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interfaces;

namespace SistemaDeVendas.Services;

public sealed class RelatorioService(SistemaDeVendasDbContext context) : IRelatorioService
{
    public Task<RelatorioExecucaoDto<VendasPorPeriodoDto>> ObterVendasPorPeriodoAsync(
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        PedidoBusinessRules.ValidarPeriodo(dataInicio, dataFim);

        // Gargalo esperado: GROUP BY em campo derivado de DataPedido. Um indice em DataPedido ajuda no filtro,
        // mas agrupamentos mensais ainda podem exigir sort/hash aggregate no SQL Server.
        var consulta = context.Pedidos
            .AsNoTracking()
            .Where(pedido => pedido.DataPedido >= dataInicio && pedido.DataPedido < dataFim)
            .Select(pedido => new
            {
                Ano = pedido.DataPedido.Year,
                Mes = pedido.DataPedido.Month,
                pedido.TotalPedido,
                pedido.ValorComissao
            })
            .GroupBy(pedido => new { pedido.Ano, pedido.Mes })
            .Select(grupo => new
            {
                grupo.Key.Ano,
                grupo.Key.Mes,
                TotalVendido = grupo.Sum(pedido => pedido.TotalPedido),
                QuantidadePedidos = grupo.Count(),
                TicketMedio = grupo.Average(pedido => pedido.TotalPedido),
                TotalComissao = grupo.Sum(pedido => pedido.ValorComissao)
            })
            .OrderBy(resultado => resultado.Ano)
            .ThenBy(resultado => resultado.Mes);

        return ExecutarMedindoAsync(async () =>
        {
            var registros = await consulta.ToListAsync(cancellationToken);

            return registros
                .Select(resultado => new VendasPorPeriodoDto(
                    resultado.Ano,
                    resultado.Mes,
                    resultado.TotalVendido,
                    resultado.QuantidadePedidos,
                    resultado.TicketMedio,
                    resultado.TotalComissao))
                .ToList();
        }, cancellationToken);
    }

    public Task<RelatorioExecucaoDto<RankingVendedorDto>> ObterRankingVendedoresAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        PedidoBusinessRules.ValidarPeriodo(dataInicio, dataFim);

        var skip = CalcularSkip(pagina, tamanhoPagina);

        // Impacto de indices: FK Pedido.IdVendedor e DataPedido reduzem leituras no JOIN e no filtro de periodo.
        // Sem eles, o ranking tende a fazer scans grandes antes da agregacao.
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
            .Skip(skip)
            .Take(tamanhoPagina);

        return ExecutarMedindoAsync(() => consulta.ToListAsync(cancellationToken), cancellationToken);
    }

    public Task<RelatorioExecucaoDto<ProdutoMaisVendidoDto>> ObterProdutosMaisVendidosAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        PedidoBusinessRules.ValidarPeriodo(dataInicio, dataFim);

        var skip = CalcularSkip(pagina, tamanhoPagina);

        // Impacto de joins: PedidoProduto costuma ser a maior tabela. Filtrar por Pedido.DataPedido antes da
        // projection evita carregar itens fora do periodo e mantem o trabalho no banco.
        var consulta = context.PedidosProdutos
            .AsNoTracking()
            .Where(item => item.Pedido.DataPedido >= dataInicio && item.Pedido.DataPedido < dataFim)
            .GroupBy(item => new
            {
                item.IdProduto,
                Produto = item.Produto.Nome,
                Categoria = item.Produto.Categoria.Nome
            })
            .Select(grupo => new ProdutoMaisVendidoDto(
                grupo.Key.IdProduto,
                grupo.Key.Produto,
                grupo.Key.Categoria,
                grupo.Sum(item => item.Quantidade),
                grupo.Sum(item => item.SubTotal),
                grupo.Average(item => item.Desconto)))
            .OrderByDescending(resultado => resultado.QuantidadeVendida)
            .ThenByDescending(resultado => resultado.Faturamento)
            .Skip(skip)
            .Take(tamanhoPagina);

        return ExecutarMedindoAsync(() => consulta.ToListAsync(cancellationToken), cancellationToken);
    }

    public Task<RelatorioExecucaoDto<ClienteMaisComprouDto>> ObterClientesQueMaisCompraramAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        PedidoBusinessRules.ValidarPeriodo(dataInicio, dataFim);

        var skip = CalcularSkip(pagina, tamanhoPagina);

        // Projection direta para DTO evita materializar Cliente e Pedido completos. Isso reduz memoria no .NET,
        // mas o banco ainda paga o custo das agregacoes e da ordenacao por TotalGasto.
        var consulta = context.Pedidos
            .AsNoTracking()
            .Where(pedido => pedido.DataPedido >= dataInicio && pedido.DataPedido < dataFim)
            .GroupBy(pedido => new { pedido.IdCliente, pedido.Cliente.Nome })
            .Select(grupo => new ClienteMaisComprouDto(
                grupo.Key.IdCliente,
                grupo.Key.Nome,
                grupo.Sum(pedido => pedido.TotalPedido),
                grupo.Count(),
                grupo.Average(pedido => pedido.TotalPedido)))
            .OrderByDescending(resultado => resultado.TotalGasto)
            .ThenBy(resultado => resultado.Cliente)
            .Skip(skip)
            .Take(tamanhoPagina);

        return ExecutarMedindoAsync(() => consulta.ToListAsync(cancellationToken), cancellationToken);
    }

    public Task<RelatorioExecucaoDto<RelatorioConsolidadoDto>> ObterRelatorioConsolidadoAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        PedidoBusinessRules.ValidarPeriodo(dataInicio, dataFim);

        var skip = CalcularSkip(pagina, tamanhoPagina);

        // Consulta propositalmente pesada para comparacao com Stored Procedure:
        // multiplos joins, filtro por data, agrupamento composto e ordenacao por agregados.
        // Indices nas FKs reduzem o custo dos joins; indices em DataPedido ajudam a restringir o periodo.
        // Agregacoes com SUM/AVG/COUNT podem gerar hash/sort aggregate e consumir tempdb em bases grandes.
        var baseConsulta =
            from pedido in context.Pedidos.AsNoTracking()
            join item in context.PedidosProdutos.AsNoTracking() on pedido.Id equals item.IdPedido
            join produto in context.Produtos.AsNoTracking() on item.IdProduto equals produto.Id
            join categoria in context.Categorias.AsNoTracking() on produto.IdCategoria equals categoria.Id
            join vendedor in context.Vendedores.AsNoTracking() on pedido.IdVendedor equals vendedor.Id
            join metodoPagamento in context.MetodosPagamento.AsNoTracking() on pedido.IdMetodoPagamento equals metodoPagamento.Id
            where pedido.DataPedido >= dataInicio && pedido.DataPedido < dataFim
            select new
            {
                IdPedido = pedido.Id,
                pedido.ValorComissao,
                Categoria = categoria.Nome,
                MetodoPagamento = metodoPagamento.Nome,
                Vendedor = vendedor.Nome,
                item.Quantidade,
                item.SubTotal,
                item.Desconto
            };

        var consultaResumo =
            from linha in baseConsulta
            group linha by new
            {
                linha.Categoria,
                linha.MetodoPagamento,
                linha.Vendedor
            }
            into grupo
            orderby grupo.Sum(x => x.SubTotal) descending,
                grupo.Key.Categoria,
                grupo.Key.MetodoPagamento,
                grupo.Key.Vendedor
            select new
            {
                grupo.Key.Categoria,
                grupo.Key.MetodoPagamento,
                grupo.Key.Vendedor,
                QuantidadePedidos = grupo.Select(x => x.IdPedido).Distinct().Count(),
                QuantidadeItensVendidos = grupo.Sum(x => x.Quantidade),
                TotalVendido = grupo.Sum(x => x.SubTotal),
                MediaDesconto = grupo.Average(x => x.Desconto)
            };

        var consultaComissoes =
            from pedidoGrupo in baseConsulta
                .Select(linha => new
                {
                    linha.Categoria,
                    linha.MetodoPagamento,
                    linha.Vendedor,
                    linha.IdPedido,
                    linha.ValorComissao
                })
                .Distinct()
            group pedidoGrupo by new
            {
                pedidoGrupo.Categoria,
                pedidoGrupo.MetodoPagamento,
                pedidoGrupo.Vendedor
            }
            into grupo
            select new
            {
                grupo.Key.Categoria,
                grupo.Key.MetodoPagamento,
                grupo.Key.Vendedor,
                TotalComissao = grupo.Sum(x => x.ValorComissao)
            };

        return ExecutarMedindoAsync(async () =>
        {
            var resumo = await consultaResumo
                .Skip(skip)
                .Take(tamanhoPagina)
                .ToListAsync(cancellationToken);

            var comissoes = await consultaComissoes.ToListAsync(cancellationToken);
            var comissaoPorGrupo = comissoes.ToDictionary(
                item => $"{item.Categoria}|{item.MetodoPagamento}|{item.Vendedor}",
                item => item.TotalComissao);

            return resumo
                .Select(item =>
                {
                    var chave = $"{item.Categoria}|{item.MetodoPagamento}|{item.Vendedor}";
                    comissaoPorGrupo.TryGetValue(chave, out var totalComissao);

                    return new RelatorioConsolidadoDto(
                        item.Categoria,
                        item.MetodoPagamento,
                        item.Vendedor,
                        item.QuantidadePedidos,
                        item.QuantidadeItensVendidos,
                        item.TotalVendido,
                        totalComissao,
                        item.MediaDesconto);
                })
                .ToList();
        }, cancellationToken);
    }

    private static async Task<RelatorioExecucaoDto<T>> ExecutarMedindoAsync<T>(
        Func<Task<List<T>>> executarConsulta,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var memoriaAntes = GC.GetTotalMemory(forceFullCollection: false);
        var stopwatch = Stopwatch.StartNew();
        var registros = await executarConsulta();
        stopwatch.Stop();
        var memoriaDepois = GC.GetTotalMemory(forceFullCollection: false);

        return new RelatorioExecucaoDto<T>(
            registros,
            stopwatch.Elapsed,
            Math.Max(0, memoriaDepois - memoriaAntes));
    }

    private static int CalcularSkip(int pagina, int tamanhoPagina)
    {
        var paginaNormalizada = Math.Max(1, pagina);
        var tamanhoNormalizado = Math.Max(1, tamanhoPagina);

        return (paginaNormalizada - 1) * tamanhoNormalizado;
    }
}
