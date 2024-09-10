using Domain.Entities.Implementations;
using Domain.Enums;

namespace Domain.Services.Interfaces;

public interface IUserService
{
    Task<User> SignUp(
        string name,
        string email,
        string password,
        Statuses status,
        DateTime birthday,
        CancellationToken ct);

    Task<User> SignIn(string email, string password, CancellationToken ct);

    Task ChangePassword(User user, string oldPassword, string newPassword, CancellationToken ct);
}