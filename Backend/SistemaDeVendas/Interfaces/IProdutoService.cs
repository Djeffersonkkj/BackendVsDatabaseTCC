using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Interfaces;

public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoResumoDto>> ListarProdutosAsync(CancellationToken cancellationToken = default);
}
