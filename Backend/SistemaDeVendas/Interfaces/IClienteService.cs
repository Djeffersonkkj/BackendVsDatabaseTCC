using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Interfaces;

// Camada Services: organiza regras de negócio e casos de uso antes de acessar repositories.
public interface IClienteService
{
    Task<IReadOnlyList<ClienteResumoDto>> ListarClientesAsync(CancellationToken cancellationToken = default);
}
