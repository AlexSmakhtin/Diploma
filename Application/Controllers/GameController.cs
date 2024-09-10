using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.DTO_s.ControllersDTO_s.Requests;
using Application.DTO_s.ControllersDTO_s.Responses;
using Application.GigaChatModels;
using Application.Services.Implementations;
using Domain.Entities.Implementations;
using Domain.Enums;
using Domain.Repositories.Interfaces;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;

namespace Application.Controllers;

[Route("api/game")]
[ApiController]
public class GameController : ControllerBase
{
    private readonly IRepository<GameTheme> _gameThemeRepository;
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly ILogger<GameController> _logger;
    private readonly HttpClient _httpClient;
    private readonly string _voiceoverEndpoint;
    private readonly PromptCreator _promptCreator;
    private readonly IAiAssistant _aiAssistant;

    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GameController(
        IAiAssistant aiAssistant,
        IUserRepository userRepository,
        IRepository<GameTheme> gameThemeRepository,
        IGameRepository gameRepository,
        ILogger<GameController> logger,
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        PromptCreator promptCreator)
    {
        ArgumentNullException.ThrowIfNull(aiAssistant);
        ArgumentNullException.ThrowIfNull(promptCreator);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(gameRepository);
        ArgumentNullException.ThrowIfNull(gameThemeRepository);
        ArgumentNullException.ThrowIfNull(logger);
        var baseUrl = configuration.GetRequiredSection("VoiceoverBaseUrl").Value;
        ArgumentNullException.ThrowIfNull(baseUrl);
        var endpoint = configuration.GetRequiredSection("VoiceoverEndpoint").Value;
        ArgumentNullException.ThrowIfNull(endpoint);
        _aiAssistant = aiAssistant;
        _voiceoverEndpoint = endpoint;
        _httpClient = httpClientFactory.CreateClient("Voiceover");
        _httpClient.BaseAddress = new Uri(baseUrl);
        _gameThemeRepository = gameThemeRepository;
        _logger = logger;
        _gameRepository = gameRepository;
        _userRepository = userRepository;
        _promptCreator = promptCreator;
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpGet("delete")]
    public async Task<IActionResult> DeleteGame(
        [FromBody] DeleteGameRequest request,
        CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(strId);
        var game = await _gameRepository.GetById(request.GameId, ct);
        if (game.User.Id != userId)
        {
            return Unauthorized("Removing avatar is not allowed for this user");
        }

        await _gameRepository.Delete(game, ct);
        return Ok();
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpGet("game_themes")]
    public async Task<ActionResult<List<GetThemesResponse>>> GetGameThemes(
        [FromQuery] string language,
        CancellationToken ct)
    {
        var themes = await _gameThemeRepository.GetAll(ct);
        return language switch
        {
            "ru" => themes.Select(e => new GetThemesResponse(e.Id, e.NameRu)).ToList(),
            "en" => themes.Select(e => new GetThemesResponse(e.Id, e.NameEn)).ToList(),
            _ => BadRequest("No provided language")
        };
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpPost("new_game")]
    public async Task<ActionResult<NewGameResponse>> NewGame([FromBody] NewGameRequest request, CancellationToken ct)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(idStr);
        var user = await _userRepository.GetById(userId, ct);
        var newGame = new Game(request.CharName, request.GameName) { User = user };
        await _gameRepository.Add(newGame, ct);
        return new NewGameResponse(newGame.Id);
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpGet("get_by_id")]
    public async Task<ActionResult<GameResponse>> GetGame([FromQuery] Guid id, CancellationToken ct)
    {
        var game = await _gameRepository.GetById(id, ct);
        return new GameResponse(game.Id, game.CharName, game.AvatarId, game.GameState);
    }

    [HttpGet("avatar/{id}")]
    public async Task<FileContentResult> GetAvatar(
        string id,
        [FromServices] IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        var httpClient = httpClientFactory.CreateClient("GigaChat");
        var bytes = await _aiAssistant.DownloadImage(id, httpClient, ct);
        return new FileContentResult(bytes, MediaTypeHeaderValue.Parse("image/jpeg"));
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpPost("avatar/remove")]
    public async Task<ActionResult> RemoveAvatar([FromBody] RemoveAvatarRequest request, CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(strId);
        var game = await _gameRepository.GetById(request.GameId, ct);
        if (game.User.Id != userId)
        {
            return Unauthorized("Removing avatar is not allowed for this user");
        }

        game.AvatarId = "";
        await _gameRepository.Update(game, ct);
        return Ok();
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpPost("generate_avatar")]
    public async Task<ActionResult<GenerateImageResponse>> GenerateAvatar(
        [FromBody] GenerateImageRequest request,
        [FromServices] IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        try
        {
            var systemPrompt = _promptCreator.ImageSystemContent();
            var game = await _gameRepository.GetById(request.GameId, ct);
            var message = _promptCreator.GenerateImage(request.Description);
            var aiRequest = new AiRequest
            {
                Model = "GigaChat-Plus",
                Messages =
                [
                    new AiMessage { Role = "system", Content = systemPrompt },
                    new AiMessage { Role = "user", Content = message }
                ]
            };
            var jsonRequestData = JsonSerializer.Serialize(aiRequest, _jsonOptions);
            var httpClient = httpClientFactory.CreateClient("GigaChat");
            var aiResponse = await _aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData, httpClient, ct);
            _logger.LogInformation("{aiResponse}", aiResponse.Choices[0].Message.Content);
            var stringContent = aiResponse.Choices[0].Message.Content;
            var startIndex = stringContent.IndexOf("<img src=\"", StringComparison.Ordinal) + "<img src=\"".Length;
            var str = stringContent.Remove(0, startIndex);
            var endIndex = str.IndexOf('"');
            var imageId = str[..endIndex];
            game.AvatarId = imageId;
            await _gameRepository.Update(game, ct);
            return new GenerateImageResponse(imageId);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning("Voiceover exception:{e}", e);
            return BadRequest(e.Message);
        }
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpPost("generate_image")]
    public async Task<ActionResult<GenerateImageResponse>> GenerateImage(
        [FromBody] GenerateSituationRequest request,
        [FromServices] IHttpClientFactory httpClientFactory,
        CancellationToken ct)
    {
        try
        {
            var systemPrompt = _promptCreator.ImageSystemContent();
            var message = _promptCreator.GenerateImage(request.Description);
            var aiRequest = new AiRequest
            {
                Model = "GigaChat-Plus",
                Messages =
                [
                    new AiMessage { Role = "system", Content = systemPrompt },
                    new AiMessage { Role = "user", Content = message }
                ]
            };
            var jsonRequestData = JsonSerializer.Serialize(aiRequest, _jsonOptions);
            var httpClient = httpClientFactory.CreateClient("GigaChat");
            var aiResponse = await _aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData, httpClient, ct);
            _logger.LogInformation("{aiResponse}", aiResponse.Choices[0].Message.Content);
            var stringContent = aiResponse.Choices[0].Message.Content;
            var startIndex = stringContent.IndexOf("<img src=\"", StringComparison.Ordinal) + "<img src=\"".Length;
            var str = stringContent.Remove(0, startIndex);
            var endIndex = str.IndexOf('"');
            var imageId = str[..endIndex];
            return new GenerateImageResponse(imageId);
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning("Voiceover exception:{e}", e);
            return BadRequest(e.Message);
        }
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpPost("voiceover")]
    public async Task<IActionResult> Voiceover(
        [FromBody] string text,
        [FromQuery] string language,
        CancellationToken ct)
    {
        try
        {
            var data = new { text = text };
            var jsonContent = JsonContent.Create(data);
            using var response =
                await _httpClient.PostAsync($"{_voiceoverEndpoint}?language={language}", jsonContent, ct);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync(ct);
            stream.Seek(0, SeekOrigin.Begin);
            var result = new byte[stream.Length];
            await stream.ReadAsync(result, ct);
            return new FileContentResult(result, MediaTypeHeaderValue.Parse("audio/mp3"));
        }
        catch (HttpRequestException e)
        {
            _logger.LogWarning("Voiceover exception:{e}", e);
            return BadRequest(e.Message);
        }
    }
}