using System.ComponentModel.DataAnnotations;

namespace Application.DTO_s.ControllersDTO_s.Requests;

public class NewGameRequest
{
    [Required] public string GameName { get; set; }
    [Required] public string CharName { get; set; }

    public NewGameRequest(
        string gameName,
        string charName) =>
        (GameName, CharName) = (gameName, charName);
}