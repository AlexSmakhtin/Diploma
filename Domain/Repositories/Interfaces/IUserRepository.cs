using Domain.Entities.Implementations;

namespace Domain.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> FindByEmail(string email, CancellationToken ct);
}