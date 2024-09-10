using Domain.Entities.Interfaces;
using Domain.Enums;

namespace Domain.Entities.Implementations;

public class Game : IEntity
{
    public Guid Id { get; init; }

    private string _charName;

    public string CharName
    {
        get => _charName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Character name is empty");
            _charName = value;
        }
    }
    
    private string _name;

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Game name is empty");
            _name = value;
        }
    }
    
    private string _avatarId;

    public string AvatarId
    {
        get => _avatarId;
        set => _avatarId = value ?? throw new ArgumentException("AvatarUrl is null");
    }
    public GameState GameState { get; set; }
    public User User { get; set; } = null!;
    public List<Message> Messages { get; set; } = [];
    public List<AiRecord> AiRecords { get; set; } = [];

    public DateTime CreationDate { get; init; }

    public Game(string charName, string name)
    {
        Name = name;
        GameState = GameState.Created;
        AvatarId = "";
        CharName = charName;
        CreationDate = DateTime.Now.ToUniversalTime();
    }
}