using Domain.Entities.Interfaces;

namespace Domain.Entities.Implementations;

public class GameTheme : IEntity
{
    public Guid Id { get; init; }

    private string _nameRu;

    public string NameRu
    {
        get => _nameRu;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _nameRu = value;
        }
    }

    private string _nameEn;
    
    public string NameEn
    {
        get => _nameEn;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _nameEn = value;
        }
    }

    public GameTheme(string nameRu, string nameEn)
    {
        NameEn = nameEn;
        NameRu = nameRu;
    }
}