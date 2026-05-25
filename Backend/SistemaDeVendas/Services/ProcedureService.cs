using System.Data;
using System.Diagnostics;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.BusinessRules;
using SistemaDeVendas.Data;
using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interfaces;

namespace SistemaDeVendas.Services;

public sealed class ProcedureService(SistemaDeVendasDbContext context) : IProcedureService
{
    public Task<ExecucaoOperacaoDto<RegistrarPedidoResultadoDto>> RegistrarPedidoAsync(
        RegistrarPedidoDto pedido,
        CancellationToken cancellationToken = default)
    {
        PedidoBusinessRules.ValidarPedido(pedido);

        return ExecutarOperacaoAsync(
            "dbo.usp_RegistrarPedido",
            command =>
            {
                command.Parameters.Add(new SqlParameter("@IdMetodoPagamento", SqlDbType.TinyInt) { Value = pedido.IdMetodoPagamento });
                command.Parameters.Add(new SqlParameter("@IdCliente", SqlDbType.Int) { Value = pedido.IdCliente });
                command.Parameters.Add(new SqlParameter("@IdVendedor", SqlDbType.Int) { Value = pedido.IdVendedor });
                command.Parameters.Add(new SqlParameter("@DataPedido", SqlDbType.DateTime) { Value = pedido.DataPedido });
                command.Parameters.Add(new SqlParameter("@Itens", SqlDbType.Structured)
                {
                    TypeName = "dbo.TipoItemPedido",
                    Value = CriarTabelaItens(pedido.Itens)
                });
            },
            async command =>
            {
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                {
                    throw new InvalidOperationException("A procedure nao retornou o resultado do pedido.");
                }

                return new RegistrarPedidoResultadoDto(
                    reader.GetInt32(reader.GetOrdinal("IdPedido")),
                    reader.GetInt32(reader.GetOrdinal("RegistrosAfetados")),
                    reader.GetDecimal(reader.GetOrdinal("TotalPedido")),
                    reader.GetDecimal(reader.GetOrdinal("ValorComissao")),
                    reader.GetString(reader.GetOrdinal("Mensagem")));
            },
            cancellationToken);
    }

    public Task<RelatorioExecucaoDto<VendasPorPeriodoDto>> ObterVendasPorPeriodoAsync(
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default)
    {
        return ExecutarRelatorioAsync(
            "dbo.usp_RelatorioVendasPorPeriodo",
            command => AdicionarPeriodo(command, dataInicio, dataFim),
            reader => new VendasPorPeriodoDto(
                reader.GetInt32(reader.GetOrdinal("Ano")),
                reader.GetInt32(reader.GetOrdinal("Mes")),
                reader.GetDecimal(reader.GetOrdinal("TotalVendido")),
                reader.GetInt32(reader.GetOrdinal("QuantidadePedidos")),
                reader.GetDecimal(reader.GetOrdinal("TicketMedio")),
                reader.GetDecimal(reader.GetOrdinal("TotalComissao"))),
            cancellationToken);
    }

    public Task<RelatorioExecucaoDto<RankingVendedorDto>> ObterRankingVendedoresAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        return ExecutarRelatorioAsync(
            "dbo.usp_RelatorioRankingVendedores",
            command =>
            {
                AdicionarPeriodo(command, dataInicio, dataFim);
                AdicionarPaginacao(command, pagina, tamanhoPagina);
            },
            reader => new RankingVendedorDto(
                reader.GetInt32(reader.GetOrdinal("IdVendedor")),
                reader.GetString(reader.GetOrdinal("Vendedor")),
                reader.GetDecimal(reader.GetOrdinal("TotalVendido")),
                reader.GetInt32(reader.GetOrdinal("QuantidadePedidos")),
                reader.GetDecimal(reader.GetOrdinal("ComissaoTotal"))),
            cancellationToken);
    }

    public Task<RelatorioExecucaoDto<ProdutoMaisVendidoDto>> ObterProdutosMaisVendidosAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        return ExecutarRelatorioAsync(
            "dbo.usp_RelatorioProdutosMaisVendidos",
            command =>
            {
                AdicionarPeriodo(command, dataInicio, dataFim);
                AdicionarPaginacao(command, pagina, tamanhoPagina);
            },
            reader => new ProdutoMaisVendidoDto(
                reader.GetInt32(reader.GetOrdinal("IdProduto")),
                reader.GetString(reader.GetOrdinal("Produto")),
                reader.GetString(reader.GetOrdinal("Categoria")),
                reader.GetInt32(reader.GetOrdinal("QuantidadeVendida")),
                reader.GetDecimal(reader.GetOrdinal("Faturamento")),
                reader.GetDecimal(reader.GetOrdinal("MediaDesconto"))),
            cancellationToken);
    }

    public Task<RelatorioExecucaoDto<ClienteMaisComprouDto>> ObterClientesQueMaisCompraramAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default)
    {
        return ExecutarRelatorioAsync(
            "dbo.usp_RelatorioClientesQueMaisCompraram",
            command =>
            {
                AdicionarPeriodo(command, dataInicio, dataFim);
                AdicionarPaginacao(command, pagina, tamanhoPagina);
            },
            reader => new ClienteMaisComprouDto(
                reader.GetInt32(reader.GetOrdinal("IdCliente")),
                reader.GetString(reader.GetOrdinal("Cliente")),
                reader.GetDecimal(reader.GetOrdinal("TotalGasto")),
                reader.GetInt32(reader.GetOrdinal("QuantidadePedidos")),
                reader.GetDecimal(reader.GetOrdinal("TicketMedio"))),
            cancellationToken);
    }

    public Task<RelatorioExecucaoDto<RelatorioConsolidadoDto>> ObterRelatorioConsolidadoAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default)
    {
        return ExecutarRelatorioAsync(
            "dbo.usp_RelatorioConsolidado",
            command =>
            {
                AdicionarPeriodo(command, dataInicio, dataFim);
                AdicionarPaginacao(command, pagina, tamanhoPagina);
            },
            reader => new RelatorioConsolidadoDto(
                reader.GetString(reader.GetOrdinal("Categoria")),
                reader.GetString(reader.GetOrdinal("MetodoPagamento")),
                reader.GetString(reader.GetOrdinal("Vendedor")),
                reader.GetInt32(reader.GetOrdinal("QuantidadePedidos")),
                reader.GetInt32(reader.GetOrdinal("QuantidadeItensVendidos")),
                reader.GetDecimal(reader.GetOrdinal("TotalVendido")),
                reader.GetDecimal(reader.GetOrdinal("TotalComissao")),
                reader.GetDecimal(reader.GetOrdinal("MediaDesconto"))),
            cancellationToken);
    }

    private async Task<ExecucaoOperacaoDto<T>> ExecutarOperacaoAsync<T>(
        string procedure,
        Action<SqlCommand> configurar,
        Func<SqlCommand, Task<T>> executar,
        CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)context.Database.GetDbConnection();
        await using var command = new SqlCommand(procedure, connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        configurar(command);

        var conexao = Stopwatch.StartNew();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }
        conexao.Stop();

        var stopwatch = Stopwatch.StartNew();
        var resultado = await executar(command);
        stopwatch.Stop();

        return new ExecucaoOperacaoDto<T>(resultado, conexao.Elapsed + stopwatch.Elapsed, stopwatch.Elapsed, stopwatch.Elapsed);
    }

    private async Task<RelatorioExecucaoDto<T>> ExecutarRelatorioAsync<T>(
        string procedure,
        Action<SqlCommand> configurar,
        Func<SqlDataReader, T> mapear,
        CancellationToken cancellationToken)
    {
        var memoriaAntes = GC.GetTotalMemory(forceFullCollection: false);
        var execucao = await ExecutarOperacaoAsync(
            procedure,
            configurar,
            async command =>
            {
                var registros = new List<T>();
                await using var reader = await command.ExecuteReaderAsync(cancellationToken);

                while (await reader.ReadAsync(cancellationToken))
                {
                    registros.Add(mapear(reader));
                }

                return registros;
            },
            cancellationToken);
        var memoriaDepois = GC.GetTotalMemory(forceFullCollection: false);

        return new RelatorioExecucaoDto<T>(
            execucao.Resultado,
            execucao.TempoExecucao,
            Math.Max(0, memoriaDepois - memoriaAntes));
    }

    private static void AdicionarPeriodo(SqlCommand command, DateTime dataInicio, DateTime dataFim)
    {
        PedidoBusinessRules.ValidarPeriodo(dataInicio, dataFim);

        command.Parameters.Add(new SqlParameter("@DataInicio", SqlDbType.DateTime) { Value = dataInicio });
        command.Parameters.Add(new SqlParameter("@DataFim", SqlDbType.DateTime) { Value = dataFim });
    }

    private static void AdicionarPaginacao(SqlCommand command, int pagina, int tamanhoPagina)
    {
        command.Parameters.Add(new SqlParameter("@Pagina", SqlDbType.Int) { Value = pagina });
        command.Parameters.Add(new SqlParameter("@TamanhoPagina", SqlDbType.Int) { Value = tamanhoPagina });
    }

    private static DataTable CriarTabelaItens(IReadOnlyList<PedidoItemDto> itens)
    {
        var tabela = new DataTable();
        tabela.Columns.Add("IdProduto", typeof(int));
        tabela.Columns.Add("Quantidade", typeof(short));
        tabela.Columns.Add("Desconto", typeof(decimal));

        foreach (var item in itens)
        {
            tabela.Rows.Add(item.IdProduto, item.Quantidade, item.Desconto);
        }

        return tabela;
    }
}
