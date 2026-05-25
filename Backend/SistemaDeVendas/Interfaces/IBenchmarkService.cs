using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Interfaces;

public interface IBenchmarkService
{
    Task<BenchmarkResultadoDto> ExecutarComparacaoAsync(int execucoes = 10, CancellationToken cancellationToken = default);
}
