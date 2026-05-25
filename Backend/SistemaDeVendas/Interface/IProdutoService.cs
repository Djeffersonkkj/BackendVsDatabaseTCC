using SistemaDeVendas.DTOs;

namespace SistemaDeVendas.Interface;

public interface IProdutoService
{
    Task<IReadOnlyList<ProdutoResumoDto>> ListarProdutosAsync(CancellationToken cancellationToken = default);
}
