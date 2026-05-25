using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SistemaDeVendas.Data;
using SistemaDeVendas.Interfaces;

namespace SistemaDeVendas.Repositories;

public class Repository<TEntity>(SistemaDeVendasDbContext context) : IRepository<TEntity>
    where TEntity : class
{
    protected readonly SistemaDeVendasDbContext Context = context;
    protected readonly DbSet<TEntity> DbSet = context.Set<TEntity>();

    public async Task<TEntity?> ObterPorIdAsync(object id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync([id], cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListarAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> BuscarAsync(Expression<Func<TEntity, bool>> filtro, CancellationToken cancellationToken = default)
    {
        return await DbSet.AsNoTracking().Where(filtro).ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public void Atualizar(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public void Remover(TEntity entity)
    {
        DbSet.Remove(entity);
    }

    public Task<int> SalvarAlteracoesAsync(CancellationToken cancellationToken = default)
    {
        return Context.SaveChangesAsync(cancellationToken);
    }
}
