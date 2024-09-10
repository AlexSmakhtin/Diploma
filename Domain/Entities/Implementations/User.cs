using System.ComponentModel.DataAnnotations;
using Domain.Entities.Interfaces;
using Domain.Enums;

namespace Domain.Entities.Implementations;

public class User: IEntity
{
    public Guid Id { get; init; }

    private string _name = null!;
    public string Name
    {
        get => _name;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Username cannot be empty", nameof(value));
            _name = value;
        }
    }
    
    private static readonly EmailAddressAttribute EmailAddressAttribute = new();
    
    private string _email = null!;
    public string Email
    {
        get => _email;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("User email cannot be empty", nameof(value));
            if (!EmailAddressAttribute.IsValid(value))
                throw new ArgumentException("User email is invalid", nameof(value));
            _email = value;
        }
    }

    public Statuses Status { get; set; }

    private DateTime _birthday;
    public DateTime Birthday
    {
        get => _birthday;
        set
        {
            if (value == default)
                throw new ArgumentException("User birthday cannot be default value", nameof(value));
            if (value > DateTime.Now.AddYears(-18))
                throw new ArgumentException("User must be at least 18 years old", nameof(value));
            _birthday = value;
        }
    }
    
    private string _hashedPassword = null!;
    public string HashedPassword
    {
        get => _hashedPassword;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("User password cannot be empty", nameof(value));
            _hashedPassword = value;
        }
    }

    public User(string name, string email, string hashedPassword, Statuses status, DateTime birthday)
    {
        Name = name;
        Email = email;
        HashedPassword = hashedPassword;
        Birthday = birthday;
        Status = status;
    }
}