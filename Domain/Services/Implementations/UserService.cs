using Domain.Entities.Implementations;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories.Interfaces;
using Domain.Services.Interfaces;

namespace Domain.Services.Implementations;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasherService _passwordHasherService;

    public UserService(IUserRepository repository, IPasswordHasherService passwordHasherService)
    {
        _userRepository = repository;
        _passwordHasherService = passwordHasherService;
    }

    public async Task<User> SignUp(
        string name,
        string email,
        string password,
        Statuses status,
        DateTime birthday,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);
        var existedUser =
            await _userRepository.FindByEmail(email, ct);
        if (existedUser != null)
            throw new UserEmailAlreadyExistsException("User email already used");
        var hashedPassword = _passwordHasherService.HashPassword(password);
        var newUser = new User(name, email, hashedPassword, status, birthday);
        await _userRepository.Add(newUser, ct);
        return newUser;
    }

    public async Task<User> SignIn(string email, string password, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        ArgumentNullException.ThrowIfNull(password);
        var existedUser =
            await _userRepository.FindByEmail(email, ct);
        if (existedUser == null)
            throw new UserNotFoundException("User not found");
        var result = _passwordHasherService.VerifyPassword(existedUser.HashedPassword, password);
        if (result == false)
            throw new IncorrectPasswordException("User password incorrect");
        return existedUser;
    }


    public async Task ChangePassword(User user, string oldPassword, string newPassword, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(oldPassword);
        ArgumentNullException.ThrowIfNull(newPassword);
        var isOldIsNew = _passwordHasherService.VerifyPassword(user.HashedPassword, newPassword);
        if (isOldIsNew)
            throw new InvalidOperationException("User new password is equal to old");
        var isVerified = _passwordHasherService.VerifyPassword(user.HashedPassword, oldPassword);
        if (isVerified == false)
            throw new InvalidOperationException("User old password doesn't match");
        var newHashedPassword = _passwordHasherService.HashPassword(newPassword);
        user.HashedPassword = newHashedPassword;
        await _userRepository.Update(user, ct);
    }
}