using SistemaDeVendas.DTOs;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Interface;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<IReadOnlyList<ClienteResumoDto>> ListarResumoAsync(CancellationToken cancellationToken = default);
}
