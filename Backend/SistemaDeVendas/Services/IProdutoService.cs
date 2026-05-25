using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Services;

public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoResumoDto>> ListarProdutosAsync(CancellationToken cancellationToken = default);
}
