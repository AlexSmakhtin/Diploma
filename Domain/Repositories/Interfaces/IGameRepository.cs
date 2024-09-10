using Domain.Entities.Implementations;

namespace Domain.Repositories.Interfaces;

public interface IGameRepository:IRepository<Game>
{
    Task<List<Game>> GetGamesByUserId(Guid userId, CancellationToken ct);
    Task<int> GetTotalGamesCount(Guid userId, CancellationToken ct);
}