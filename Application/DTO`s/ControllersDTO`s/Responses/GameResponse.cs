using Domain.Enums;

namespace Application.DTO_s.ControllersDTO_s.Responses;

public record GameResponse(Guid GameId, string CharName, string AvatarId, GameState GameState);
