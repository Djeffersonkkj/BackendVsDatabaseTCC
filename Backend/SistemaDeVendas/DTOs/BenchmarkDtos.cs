namespace SistemaDeVendas.DTOs;

public sealed record BenchmarkCenarioDto(
    RegistrarPedidoDto Pedido,
    DateTime DataInicioRelatorio,
    DateTime DataFimRelatorio,
    int Pagina,
    int TamanhoPagina,
    int Execucoes);

public sealed record EstatisticaTempoDto(
    double MediaMs,
    double MedianaMs,
    double MinimoMs,
    double MaximoMs,
    double DesvioPadraoMs);

public sealed record BenchmarkOperacaoDto(
    string Operacao,
    EstatisticaTempoDto Backend,
    EstatisticaTempoDto Procedure,
    int RegistrosBackend,
    int RegistrosProcedure,
    int Execucoes,
    bool ResultadosEquivalentes,
    EstatisticaTempoDto? ProcessamentoBackend = null,
    EstatisticaTempoDto? PersistenciaBackend = null,
    EstatisticaTempoDto? ProcessamentoProcedure = null,
    EstatisticaTempoDto? PersistenciaProcedure = null)
{
    public double DiferencaPercentual =>
        Backend.MediaMs <= 0 ? 0 : ((Procedure.MediaMs - Backend.MediaMs) / Backend.MediaMs) * 100;
}

public sealed record BenchmarkResultadoDto(
    BenchmarkCenarioDto Cenario,
    IReadOnlyList<BenchmarkOperacaoDto> Operacoes,
    TimeSpan TempoConexaoBackend,
    TimeSpan TempoConexaoProcedure,
    string EstrategiaReproducibilidade);
