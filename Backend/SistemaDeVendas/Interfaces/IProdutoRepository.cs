using SistemaDeVendas.DTOs;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Interfaces;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<IReadOnlyList<ProdutoResumoDto>> ListarComCategoriaAsync(CancellationToken cancellationToken = default);
}
