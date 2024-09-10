using Domain.Entities.Interfaces;

namespace Domain.Entities.Implementations;

public class Message : IEntity
{
    public Guid Id { get; init; }

    public DateTime CreationDate { get; init; }

    private string _role;

    public string Role
    {
        get => _role;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _role = value;
        }
    }

    private string _content;

    public string Content
    {
        get => _content;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Content is empty or null", nameof(value));
            _content = value;
        }
    }

    public Game Game { get; set; } = null!;

    public Message(string content, string role)
    {
        CreationDate = DateTime.Now.ToUniversalTime();
        Role = role;
        Content = content;
    }
}