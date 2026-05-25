namespace SistemaDeVendas.DTOs;

public sealed record RelatorioExecucaoDto<T>(
    IReadOnlyList<T> Registros,
    TimeSpan TempoConsulta,
    long MemoriaUtilizadaBytes)
{
    public int QuantidadeRegistros => Registros.Count;
}

public sealed record VendasPorPeriodoDto(
    int Ano,
    int Mes,
    decimal TotalVendido,
    int QuantidadePedidos,
    decimal TicketMedio,
    decimal TotalComissao);

public sealed record RankingVendedorDto(
    int IdVendedor,
    string Vendedor,
    decimal TotalVendido,
    int QuantidadePedidos,
    decimal ComissaoTotal);

public sealed record ProdutoMaisVendidoDto(
    int IdProduto,
    string Produto,
    string Categoria,
    int QuantidadeVendida,
    decimal Faturamento,
    decimal MediaDesconto);

public sealed record ClienteMaisComprouDto(
    int IdCliente,
    string Cliente,
    decimal TotalGasto,
    int QuantidadePedidos,
    decimal TicketMedio);

public sealed record RelatorioConsolidadoDto(
    string Categoria,
    string MetodoPagamento,
    string Vendedor,
    int QuantidadePedidos,
    int QuantidadeItensVendidos,
    decimal TotalVendido,
    decimal TotalComissao,
    decimal MediaDesconto);
