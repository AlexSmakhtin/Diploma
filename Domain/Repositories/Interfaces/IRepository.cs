using Domain.Entities.Interfaces;

namespace Domain.Repositories.Interfaces;

public interface IRepository<TEntity> where TEntity : IEntity
{
    Task<TEntity> GetById(Guid id, CancellationToken ct);
    Task<List<TEntity>> GetAll(CancellationToken ct);
    Task Add(TEntity entity, CancellationToken ct);
    Task Update(TEntity entity, CancellationToken ct);
    Task Delete(TEntity entity, CancellationToken ct);
}