using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.Data;
using SistemaDeVendas.DTOs;
using SistemaDeVendas.Interfaces;
using SistemaDeVendas.Models;

namespace SistemaDeVendas.Repositories;

public sealed class ProdutoRepository(SistemaDeVendasDbContext context)
    : Repository<Produto>(context), IProdutoRepository
{
    public async Task<IReadOnlyList<ProdutoResumoDto>> ListarComCategoriaAsync(CancellationToken cancellationToken = default)
    {
        return await Context.Produtos
            .AsNoTracking()
            .OrderBy(produto => produto.Nome)
            .Select(produto => new ProdutoResumoDto(
                produto.Id,
                produto.Nome,
                produto.Categoria.Nome,
                produto.Estoque,
                produto.PrecoUnitario))
            .ToListAsync(cancellationToken);
    }
}
