using SistemaDeVendas.DTOs;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Interfaces;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<IReadOnlyList<ClienteResumoDto>> ListarResumoAsync(CancellationToken cancellationToken = default);
}
