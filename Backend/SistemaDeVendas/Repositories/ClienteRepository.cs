using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.Data;
using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interface;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Repositories;

public sealed class ClienteRepository(SistemaDeVendasDbContext context)
    : Repository<Cliente>(context), IClienteRepository
{
    public async Task<IReadOnlyList<ClienteResumoDto>> ListarResumoAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Clientes
            .AsNoTracking()
            .OrderBy(cliente => cliente.Nome)
            .Select(cliente => new ClienteResumoDto(
                cliente.Id,
                cliente.Nome,
                cliente.Email,
                cliente.EnderecosClientes.Count,
                cliente.Pedidos.Count))
            .ToListAsync(cancellationToken);
    }
}
