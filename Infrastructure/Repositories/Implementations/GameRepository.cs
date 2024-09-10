using Domain.Entities.Implementations;
using Domain.Repositories.Interfaces;
using Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implementations;

public class GameRepository : IGameRepository
{
    private readonly PostgreDbContext _dbContext;
    private DbSet<Game> Games => _dbContext.Set<Game>();

    public GameRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Game> GetById(Guid id, CancellationToken ct)
    {
        return await Games
            .Include(e => e.AiRecords)
            .Include(e => e.Messages)
            .AsSplitQuery()
            .FirstAsync(e => e.Id == id, ct);
    }

    public async Task<List<Game>> GetAll(CancellationToken ct)
    {
        return await Games.ToListAsync(ct);
    }

    public async Task Add(Game entity, CancellationToken ct)
    {
        await Games.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(Game entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(Game entity, CancellationToken ct)
    {
        Games.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task<List<Game>> GetGamesByUserId(Guid userId, CancellationToken ct)
    {
        return await Games
            .Include(e => e.User)
            .Include(e => e.Messages)
            .Where(e => e.User.Id == userId)
            .AsSplitQuery()
            .ToListAsync(ct);
    }

    public async Task<int> GetTotalGamesCount(Guid userId, CancellationToken ct)
    {
        return await Games
            .Include(e => e.User)
            .Where(e => e.User.Id == userId)
            .CountAsync(ct);
    }
}