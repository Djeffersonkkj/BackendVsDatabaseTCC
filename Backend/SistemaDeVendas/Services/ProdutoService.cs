using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interface;

namespace SistemaDeVendas.Services;

public sealed class ProdutoService(IProdutoRepository produtoRepository) : IProdutoService
{
    public Task<IReadOnlyList<ProdutoResumoDto>> ListarProdutosAsync(CancellationToken cancellationToken = default)
    {
        return produtoRepository.ListarComCategoriaAsync(cancellationToken);
    }
}
