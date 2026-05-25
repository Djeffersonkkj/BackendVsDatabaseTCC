using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interface;

namespace SistemaDeVendas.Services;

public sealed class ClienteService(IClienteRepository clienteRepository) : IClienteService
{
    public Task<IReadOnlyList<ClienteResumoDto>> ListarClientesAsync(CancellationToken cancellationToken = default)
    {
        return clienteRepository.ListarResumoAsync(cancellationToken);
    }
}
