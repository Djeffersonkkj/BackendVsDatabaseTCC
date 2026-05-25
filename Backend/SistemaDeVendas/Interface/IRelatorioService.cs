using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Interface;

public interface IRelatorioService
{
    Task<RelatorioExecucaoDto<VendasPorPeriodoDto>> ObterVendasPorPeriodoAsync(
        DateTime dataInicio,
        DateTime dataFim,
        CancellationToken cancellationToken = default);

    Task<RelatorioExecucaoDto<RankingVendedorDto>> ObterRankingVendedoresAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default);

    Task<RelatorioExecucaoDto<ProdutoMaisVendidoDto>> ObterProdutosMaisVendidosAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default);

    Task<RelatorioExecucaoDto<ClienteMaisComprouDto>> ObterClientesQueMaisCompraramAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 10,
        CancellationToken cancellationToken = default);

    Task<RelatorioExecucaoDto<RelatorioConsolidadoDto>> ObterRelatorioConsolidadoAsync(
        DateTime dataInicio,
        DateTime dataFim,
        int pagina = 1,
        int tamanhoPagina = 20,
        CancellationToken cancellationToken = default);
}
