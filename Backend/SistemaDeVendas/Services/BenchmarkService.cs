using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.Data;
using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interfaces;

namespace SistemaDeVendas.Services;

public sealed class BenchmarkService(
    SistemaDeVendasDbContext context,
    IPedidoService pedidoService,
    IProcedureService procedureService,
    IRelatorioService relatorioService) : IBenchmarkService
{
    private const int Warmups = 2;

    public async Task<BenchmarkResultadoDto> ExecutarComparacaoAsync(
        int execucoes = 10,
        CancellationToken cancellationToken = default)
    {
        var cenario = await CriarCenarioFixoAsync(Math.Max(1, execucoes), cancellationToken);
        var estoqueOriginal = await CapturarEstoqueAsync(cenario.Pedido, cancellationToken);

        var tempoConexaoBackend = await MedirConexaoAsync(cancellationToken);
        var tempoConexaoProcedure = await MedirConexaoAsync(cancellationToken);

        await ExecutarWarmupAsync(cenario, estoqueOriginal, cancellationToken);

        var operacoes = new List<BenchmarkOperacaoDto>
        {
            await MedirPedidoAsync(cenario, estoqueOriginal, cancellationToken),
            await MedirRelatorioAsync(
                "Relatorio consolidado",
                () => relatorioService.ObterRelatorioConsolidadoAsync(cenario.DataInicioRelatorio, cenario.DataFimRelatorio, cenario.Pagina, cenario.TamanhoPagina, cancellationToken),
                () => procedureService.ObterRelatorioConsolidadoAsync(cenario.DataInicioRelatorio, cenario.DataFimRelatorio, cenario.Pagina, cenario.TamanhoPagina, cancellationToken),
                cenario.Execucoes,
                cancellationToken),
            await MedirRelatorioAsync(
                "Relatorio vendas por periodo",
                () => relatorioService.ObterVendasPorPeriodoAsync(cenario.DataInicioRelatorio, cenario.DataFimRelatorio, cancellationToken),
                () => procedureService.ObterVendasPorPeriodoAsync(cenario.DataInicioRelatorio, cenario.DataFimRelatorio, cancellationToken),
                cenario.Execucoes,
                cancellationToken)
        };

        return new BenchmarkResultadoDto(
            cenario,
            operacoes,
            tempoConexaoBackend,
            tempoConexaoProcedure,
            "Cada execucao usa o mesmo cliente, vendedor, metodo, data e itens. Apos registrar o pedido, o benchmark remove o pedido criado e restaura o estoque original dos produtos do cenario. Isso evita dependencia de execucoes anteriores, mas nao zera identity/cache/plano do SQL Server.");
    }

    private async Task<BenchmarkOperacaoDto> MedirPedidoAsync(
        BenchmarkCenarioDto cenario,
        IReadOnlyDictionary<int, int> estoqueOriginal,
        CancellationToken cancellationToken)
    {
        var temposBackend = new List<double>();
        var temposProcedure = new List<double>();
        var processamentoBackend = new List<double>();
        var persistenciaBackend = new List<double>();
        var processamentoProcedure = new List<double>();
        var persistenciaProcedure = new List<double>();
        var registrosBackend = 0;
        var registrosProcedure = 0;
        var equivalente = true;

        for (var i = 0; i < cenario.Execucoes; i++)
        {
            await RestaurarCenarioAsync(cenario.Pedido, estoqueOriginal, cancellationToken);
            var totalBackendStopwatch = Stopwatch.StartNew();
            var backend = await pedidoService.RegistrarPedidoAsync(cenario.Pedido, cancellationToken);
            totalBackendStopwatch.Stop();
            temposBackend.Add(totalBackendStopwatch.Elapsed.TotalMilliseconds);
            processamentoBackend.Add(backend.TempoProcessamento.TotalMilliseconds);
            persistenciaBackend.Add(backend.TempoPersistencia.TotalMilliseconds);
            registrosBackend = backend.Resultado.RegistrosAfetados;
            var totalBackend = backend.Resultado.TotalPedido;
            var comissaoBackend = backend.Resultado.ValorComissao;
            await RemoverPedidoAsync(backend.Resultado.IdPedido, cancellationToken);
            await RestaurarCenarioAsync(cenario.Pedido, estoqueOriginal, cancellationToken);

            var totalProcedureStopwatch = Stopwatch.StartNew();
            var procedure = await procedureService.RegistrarPedidoAsync(cenario.Pedido, cancellationToken);
            totalProcedureStopwatch.Stop();
            temposProcedure.Add(totalProcedureStopwatch.Elapsed.TotalMilliseconds);
            processamentoProcedure.Add(procedure.TempoProcessamento.TotalMilliseconds);
            persistenciaProcedure.Add(procedure.TempoPersistencia.TotalMilliseconds);
            registrosProcedure = procedure.Resultado.RegistrosAfetados;
            equivalente &= totalBackend == procedure.Resultado.TotalPedido
                && comissaoBackend == procedure.Resultado.ValorComissao
                && registrosBackend == registrosProcedure;
            await RemoverPedidoAsync(procedure.Resultado.IdPedido, cancellationToken);
        }

        await RestaurarCenarioAsync(cenario.Pedido, estoqueOriginal, cancellationToken);

        return new BenchmarkOperacaoDto(
            "RegistrarPedido",
            CalcularEstatistica(temposBackend),
            CalcularEstatistica(temposProcedure),
            registrosBackend,
            registrosProcedure,
            cenario.Execucoes,
            equivalente,
            CalcularEstatistica(processamentoBackend),
            CalcularEstatistica(persistenciaBackend),
            CalcularEstatistica(processamentoProcedure),
            CalcularEstatistica(persistenciaProcedure));
    }

    private static async Task<BenchmarkOperacaoDto> MedirRelatorioAsync<T>(
        string operacao,
        Func<Task<RelatorioExecucaoDto<T>>> executarBackend,
        Func<Task<RelatorioExecucaoDto<T>>> executarProcedure,
        int execucoes,
        CancellationToken cancellationToken)
    {
        var temposBackend = new List<double>();
        var temposProcedure = new List<double>();
        var registrosBackend = 0;
        var registrosProcedure = 0;
        var equivalente = true;

        for (var i = 0; i < execucoes; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var totalBackendStopwatch = Stopwatch.StartNew();
            var backend = await executarBackend();
            totalBackendStopwatch.Stop();

            var totalProcedureStopwatch = Stopwatch.StartNew();
            var procedure = await executarProcedure();
            totalProcedureStopwatch.Stop();

            temposBackend.Add(totalBackendStopwatch.Elapsed.TotalMilliseconds);
            temposProcedure.Add(totalProcedureStopwatch.Elapsed.TotalMilliseconds);
            registrosBackend = backend.QuantidadeRegistros;
            registrosProcedure = procedure.QuantidadeRegistros;
            equivalente &= backend.QuantidadeRegistros == procedure.QuantidadeRegistros;
        }

        return new BenchmarkOperacaoDto(
            operacao,
            CalcularEstatistica(temposBackend),
            CalcularEstatistica(temposProcedure),
            registrosBackend,
            registrosProcedure,
            execucoes,
            equivalente);
    }

    private async Task ExecutarWarmupAsync(
        BenchmarkCenarioDto cenario,
        IReadOnlyDictionary<int, int> estoqueOriginal,
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < Warmups; i++)
        {
            await RestaurarCenarioAsync(cenario.Pedido, estoqueOriginal, cancellationToken);
            var backend = await pedidoService.RegistrarPedidoAsync(cenario.Pedido, cancellationToken);
            await RemoverPedidoAsync(backend.Resultado.IdPedido, cancellationToken);
            await RestaurarCenarioAsync(cenario.Pedido, estoqueOriginal, cancellationToken);

            var procedure = await procedureService.RegistrarPedidoAsync(cenario.Pedido, cancellationToken);
            await RemoverPedidoAsync(procedure.Resultado.IdPedido, cancellationToken);
            await RestaurarCenarioAsync(cenario.Pedido, estoqueOriginal, cancellationToken);

            await relatorioService.ObterRelatorioConsolidadoAsync(cenario.DataInicioRelatorio, cenario.DataFimRelatorio, cenario.Pagina, cenario.TamanhoPagina, cancellationToken);
            await procedureService.ObterRelatorioConsolidadoAsync(cenario.DataInicioRelatorio, cenario.DataFimRelatorio, cenario.Pagina, cenario.TamanhoPagina, cancellationToken);
        }
    }

    private async Task<BenchmarkCenarioDto> CriarCenarioFixoAsync(int execucoes, CancellationToken cancellationToken)
    {
        var cliente = await context.Clientes.AsNoTracking().OrderBy(cliente => cliente.Id).FirstAsync(cancellationToken);
        var vendedor = await context.Vendedores.AsNoTracking().OrderBy(vendedor => vendedor.Id).FirstAsync(cancellationToken);
        var metodo = await context.MetodosPagamento.AsNoTracking().OrderBy(metodo => metodo.Id).FirstAsync(cancellationToken);
        var itens = await context.Produtos
            .AsNoTracking()
            .Where(produto => produto.Estoque >= 50)
            .OrderBy(produto => produto.Id)
            .Take(3)
            .Select(produto => new PedidoItemDto(produto.Id, 2, 5))
            .ToListAsync(cancellationToken);

        if (itens.Count < 3)
        {
            throw new InvalidOperationException("Nao ha tres produtos com estoque >= 50 para executar benchmark reproduzivel.");
        }

        return new BenchmarkCenarioDto(
            new RegistrarPedidoDto(metodo.Id, cliente.Id, vendedor.Id, new DateTime(2026, 1, 15, 10, 0, 0), itens),
            new DateTime(2024, 1, 1),
            new DateTime(2027, 1, 1),
            1,
            20,
            execucoes);
    }

    private async Task<IReadOnlyDictionary<int, int>> CapturarEstoqueAsync(
        RegistrarPedidoDto pedido,
        CancellationToken cancellationToken)
    {
        var ids = pedido.Itens.Select(item => item.IdProduto).ToList();
        return await context.Produtos
            .AsNoTracking()
            .Where(produto => ids.Contains(produto.Id))
            .ToDictionaryAsync(produto => produto.Id, produto => produto.Estoque, cancellationToken);
    }

    private async Task RestaurarCenarioAsync(
        RegistrarPedidoDto pedido,
        IReadOnlyDictionary<int, int> estoqueOriginal,
        CancellationToken cancellationToken)
    {
        var ids = pedido.Itens.Select(item => item.IdProduto).ToList();
        var produtos = await context.Produtos.Where(produto => ids.Contains(produto.Id)).ToListAsync(cancellationToken);

        foreach (var produto in produtos)
        {
            produto.Estoque = estoqueOriginal[produto.Id];
        }

        await context.SaveChangesAsync(cancellationToken);
        context.ChangeTracker.Clear();
    }

    private async Task RemoverPedidoAsync(int idPedido, CancellationToken cancellationToken)
    {
        await context.PedidosProdutos
            .Where(item => item.IdPedido == idPedido)
            .ExecuteDeleteAsync(cancellationToken);
        await context.Pedidos
            .Where(pedido => pedido.Id == idPedido)
            .ExecuteDeleteAsync(cancellationToken);
        context.ChangeTracker.Clear();
    }

    private async Task<TimeSpan> MedirConexaoAsync(CancellationToken cancellationToken)
    {
        var connection = context.Database.GetDbConnection();
        var estavaFechada = connection.State != System.Data.ConnectionState.Open;
        var stopwatch = Stopwatch.StartNew();

        if (estavaFechada)
        {
            await connection.OpenAsync(cancellationToken);
        }

        stopwatch.Stop();

        return stopwatch.Elapsed;
    }

    private static EstatisticaTempoDto CalcularEstatistica(IReadOnlyList<double> valores)
    {
        var ordenados = valores.Order().ToArray();
        var media = ordenados.Average();
        var mediana = ordenados.Length % 2 == 1
            ? ordenados[ordenados.Length / 2]
            : (ordenados[ordenados.Length / 2 - 1] + ordenados[ordenados.Length / 2]) / 2;
        var variancia = ordenados.Sum(valor => Math.Pow(valor - media, 2)) / ordenados.Length;

        return new EstatisticaTempoDto(media, mediana, ordenados.First(), ordenados.Last(), Math.Sqrt(variancia));
    }
}
