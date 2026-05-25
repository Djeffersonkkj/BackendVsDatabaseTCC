namespace SistemaDeVendas.Interfaces;

public interface ISqlComparacaoService
{
    Task<string> SalvarSqlEfCoreAsync(DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default);
}
