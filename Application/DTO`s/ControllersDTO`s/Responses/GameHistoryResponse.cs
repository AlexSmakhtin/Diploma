namespace Application.DTO_s.ControllersDTO_s.Responses;

public record GameHistoryResponse(Guid Id, string Name, string CreationDate, int MessagesCount, string GameState);