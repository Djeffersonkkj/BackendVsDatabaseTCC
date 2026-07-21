namespace SistemaDeVendas.DTOs;

public sealed record BenchmarkCenarioDto(
    RegistrarPedidoDto Pedido,
    DateTime DataInicioRelatorio,
    DateTime DataFimRelatorio,
    int Pagina,
    int TamanhoPagina,
    int Execucoes);

public sealed record BenchmarkOperacaoDto(
    string Operacao,
    IReadOnlyList<double> TemposBackendMs,
    IReadOnlyList<double> TemposProcedureMs,
    int RegistrosBackend,
    int RegistrosProcedure,
    int Execucoes,
    bool ResultadosEquivalentes,
    IReadOnlyList<double>? TemposProcessamentoBackendMs = null,
    IReadOnlyList<double>? TemposPersistenciaBackendMs = null,
    IReadOnlyList<double>? TemposProcessamentoProcedureMs = null,
    IReadOnlyList<double>? TemposPersistenciaProcedureMs = null);

public sealed record BenchmarkResultadoDto(
    BenchmarkCenarioDto Cenario,
    IReadOnlyList<BenchmarkOperacaoDto> Operacoes,
    TimeSpan TempoConexaoBackend,
    TimeSpan TempoConexaoProcedure,
    string EstrategiaReproducibilidade);
