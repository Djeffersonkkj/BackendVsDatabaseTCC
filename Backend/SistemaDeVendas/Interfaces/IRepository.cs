using System.Linq.Expressions;

namespace SistemaDeVendas.Interfaces;

// Camada Repositories: encapsula operações básicas de persistência e consulta das entidades.
public interface IRepository<TEntity>
    where TEntity : class
{
    Task<TEntity?> ObterPorIdAsync(object id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> ListarAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> BuscarAsync(Expression<Func<TEntity, bool>> filtro, CancellationToken cancellationToken = default);
    Task AdicionarAsync(TEntity entity, CancellationToken cancellationToken = default);
    void Atualizar(TEntity entity);
    void Remover(TEntity entity);
    Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default);
}
