using Domain.Entities.Implementations;
using Domain.Repositories.Interfaces;
using Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Implementations;

public class UserRepository: IUserRepository
{
    private readonly PostgreDbContext _dbContext;
    private DbSet<User> Users => _dbContext.Set<User>();

    public UserRepository(PostgreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User> GetById(Guid id, CancellationToken ct)
    {
        return Users.FirstAsync(e => e.Id == id, ct);
    }

    public async Task<List<User>> GetAll(CancellationToken ct)
    {
        return await Users.ToListAsync(ct);
    }

    public async Task Add(User entity, CancellationToken ct)
    {
        await Users.AddAsync(entity, ct);
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Update(User entity, CancellationToken ct)
    {
        _dbContext.Entry(entity).State = EntityState.Modified;
        await _dbContext.SaveChangesAsync(ct);
    }

    public async Task Delete(User entity, CancellationToken ct)
    {
        Users.Remove(entity);
        await _dbContext.SaveChangesAsync(ct);
    }

    public Task<User?> FindByEmail(string email, CancellationToken ct)
    {
        return Users.FirstOrDefaultAsync(e => e.Email == email, ct);
    }
}