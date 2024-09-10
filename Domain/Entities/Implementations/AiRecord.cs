using Domain.Entities.Interfaces;

namespace Domain.Entities.Implementations;

public class AiRecord : IEntity
{
    public Guid Id { get; init; }
    public DateTime CreationDate { get; init; }
    private string _role;

    public string Role
    {
        get => _role;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("AiRecord role is empty or null", nameof(value));
            _role = value;
        }
    }

    private string _content;

    public string Content
    {
        get => _content;
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("AiRecord content is empty", nameof(value));
            _content = value;
        }
    }

    public Game Game { get; set; } = null!;

    public AiRecord(string role, string content)
    {
        CreationDate = DateTime.Now.ToUniversalTime();
        Role = role;
        Content = content;
    }
}