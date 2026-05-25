using SistemaDeVendas.DTOs;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<IReadOnlyList<ClienteResumoDto>> ListarResumoAsync(CancellationToken cancellationToken = default);
}
