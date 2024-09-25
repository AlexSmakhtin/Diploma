using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Application.DTO_s.SignalRDTO_s.Requests;
using Application.DTO_s.SignalRDTO_s.Responses;
using Application.GigaChatModels;
using Application.Services.Implementations;
using Domain.Entities.Implementations;
using Domain.Enums;
using Domain.Repositories.Interfaces;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NodaTime;
using SignalRSwaggerGen.Attributes;
using Message = Domain.Entities.Implementations.Message;

namespace Application.SignalR.Hub;

[SignalRHub]
public class ChatHub : Microsoft.AspNetCore.SignalR.Hub
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        AllowTrailingCommas = true
    };

    [SignalRMethod]
    [Authorize]
    public async Task ContinueGame(
        ContinueGameRequest request,
        [SignalRHidden] [FromServices] IHttpClientFactory httpClientFactory,
        [SignalRHidden] [FromServices] PromptCreator promptCreator,
        [SignalRHidden] [FromServices] IAiAssistant aiAssistant,
        [SignalRHidden] [FromServices] IRepository<Message> messageRepository,
        [SignalRHidden] [FromServices] IGameRepository gameRepository,
        [SignalRHidden] [FromServices] CancellationToken ct = default)
    {
        Console.WriteLine("request");
        ArgumentNullException.ThrowIfNull(messageRepository);
        ArgumentNullException.ThrowIfNull(aiAssistant);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(promptCreator);
        ArgumentNullException.ThrowIfNull(gameRepository);
        var game = await gameRepository.GetById(request.GameId, ct);
        var messages = game.Messages
            .OrderBy(e => e.CreationDate)
            .Select(e => new
            {
                e.Role,
                e.Content,
                CreationDate = Instant
                    .FromDateTimeUtc(e.CreationDate)
                    .InZone(DateTimeZoneProviders.Tzdb[request.TimeZone])
                    .ToDateTimeUnspecified()
                    .ToString("HH:mm")
            })
            .ToList();
        await Clients.Caller.SendAsync("ContinueGame",
            new { messages }, ct);
        var httpClient = httpClientFactory.CreateClient("GigaChat");
        Console.WriteLine(game.GameState);
        Console.WriteLine("-----------------------------------------------------------------------------");
        switch (game.GameState)
        {
            case GameState.WithLocation:
            {
                var locationPrompt = promptCreator.GenerateLocations(request.Language);
                var aiRequest = new AiRequest
                {
                    Model = "GigaChat-Plus",
                    Messages =
                    [
                        new AiMessage { Content = locationPrompt, Role = "user" }
                    ]
                };
                var jsonRequestData = JsonSerializer.Serialize(aiRequest, _jsonOptions);
                while (aiAssistant.QueueCount != 0)
                {
                    await Clients.Caller.SendAsync("ReceiveQueueCount", new { count = aiAssistant.QueueCount }, ct);
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }

                var response = await aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData, httpClient, ct);
                var responseData = response.Choices[0].Message;
                var data = JsonSerializer.Deserialize<LocationsResponse>(responseData.Content);

                await Clients.Caller.SendAsync("ReceiveLocations", data, ct);
                Console.WriteLine("request1");
                break;
            }
            case GameState.InProcess:
            {
                var systemPrompt = promptCreator.GameSystemContent();
                var lastContent = game.AiRecords.OrderBy(e => e.CreationDate).Last().Content;
                var userPrompt = promptCreator.GetThreeAnswers(lastContent, request.Language);
                var requestData = new AiRequest
                {
                    Model = "GigaChat-Plus",
                    Messages =
                    [
                        new AiMessage { Role = "system", Content = systemPrompt },
                        new AiMessage { Role = "user", Content = userPrompt }
                    ]
                };
                var jsonRequestData2 = JsonSerializer.Serialize(requestData, _jsonOptions);
                while (aiAssistant.QueueCount != 0)
                {
                    await Clients.Caller.SendAsync("ReceiveQueueCount", new { count = aiAssistant.QueueCount }, ct);
                    await Task.Delay(TimeSpan.FromSeconds(2), ct);
                }

                var response2 = await aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData2, httpClient, ct);
                var aiMessage2 = response2.Choices[0].Message;
                var data = JsonSerializer.Deserialize<ChoiceResponse>(aiMessage2.Content);
                await Clients.Caller.SendAsync("ReceiveChoices", data, ct);
                Console.WriteLine("request2");
                break;
            }
            default:
            {
                throw new InvalidOperationException("GameState is invalid");
            }
        }
    }


    [SignalRMethod]
    [Authorize]
    public async Task SendLocations(
        SendLocationsRequest request,
        [SignalRHidden] [FromServices] IHttpClientFactory httpClientFactory,
        [SignalRHidden] [FromServices] PromptCreator promptCreator,
        [SignalRHidden] [FromServices] IAiAssistant aiAssistant,
        [SignalRHidden] [FromServices] IRepository<Message> messageRepository,
        [SignalRHidden] [FromServices] IGameRepository gameRepository,
        [SignalRHidden] [FromServices] CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(messageRepository);
        ArgumentNullException.ThrowIfNull(aiAssistant);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(promptCreator);
        ArgumentNullException.ThrowIfNull(gameRepository);
        var game = await gameRepository.GetById(request.GameId, ct);
        var locationPrompt = promptCreator.GenerateLocations(request.Language);
        var aiRequest = new AiRequest
        {
            Model = "GigaChat-Plus",
            Messages =
            [
                new AiMessage { Content = locationPrompt, Role = "user" }
            ]
        };
        var jsonRequestData = JsonSerializer.Serialize(aiRequest, _jsonOptions);
        var httpClient = httpClientFactory.CreateClient("GigaChat");
        while (aiAssistant.QueueCount != 0)
        {
            await Clients.Caller.SendAsync("ReceiveQueueCount", new { count = aiAssistant.QueueCount }, ct);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        var message = new Message(request.Language.Contains("ru")
            ? "Приветствую, прежде чем начать игру выбери стартовую локацию"
            : "Welcome, before you start the game, select a starting location", "assistant");
        game.Messages.Add(message);
        game.GameState = GameState.WithLocation;
        await gameRepository.Update(game, ct);
        await Clients.Caller.SendAsync("ReceiveGameState", game.GameState, ct);
        await Clients.Caller.SendAsync(
            "ReceiveMessage",
            new
            {
                role = message.Role,
                content = message.Content,
                creationDate = Instant
                    .FromDateTimeUtc(DateTime.Now.ToUniversalTime())
                    .InZone(DateTimeZoneProviders.Tzdb[request.TimeZone])
                    .ToDateTimeUnspecified()
                    .ToString("HH:mm")
            }
            , ct);
        await Clients.Caller.SendAsync("ReceiveMessageInProcess", ct);
        var response = await aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData, httpClient, ct);
        var responseData = response.Choices[0].Message;
        var data = JsonSerializer.Deserialize<LocationsResponse>(responseData.Content);
        await Clients.Caller.SendAsync("ReceiveLocations", data, ct);
    }

    [SignalRMethod]
    [Authorize]
    public async Task AcceptChoice(
        AcceptChoiceRequest request,
        [SignalRHidden] [FromServices] IHttpClientFactory httpClientFactory,
        [SignalRHidden] [FromServices] PromptCreator promptCreator,
        [SignalRHidden] [FromServices] IAiAssistant aiAssistant,
        [SignalRHidden] [FromServices] IRepository<Message> messageRepository,
        [SignalRHidden] [FromServices] IGameRepository gameRepository,
        [SignalRHidden] [FromServices] CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(messageRepository);
        ArgumentNullException.ThrowIfNull(aiAssistant);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(promptCreator);
        ArgumentNullException.ThrowIfNull(gameRepository);
        var game = await gameRepository.GetById(request.GameId, ct);
        game.Messages.Add(new Message(request.Choice, "user") { Game = game });
        var userPrompt = promptCreator.AcceptChoice(request.Choice, request.Language);
        game.AiRecords.Add(new AiRecord("user", userPrompt) { Game = game });
        var aiMessages = game.AiRecords
            .OrderBy(e => e.CreationDate)
            .Select(e => new AiMessage
            {
                Role = e.Role,
                Content = e.Content
            })
            .ToList();
        var requestData = new AiRequest
        {
            Model = "GigaChat-Plus",
            Messages = aiMessages
        };
        var jsonRequestData = JsonSerializer.Serialize(requestData, _jsonOptions);
        var httpClient = httpClientFactory.CreateClient("GigaChat");
        while (aiAssistant.QueueCount != 0)
        {
            await Clients.Caller.SendAsync("ReceiveQueueCount", new { count = aiAssistant.QueueCount }, ct);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        await Clients.Caller.SendAsync("ReceiveMessageInProcess", ct);
        var response = await aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData, httpClient, ct);
        var aiMessage = response.Choices[0].Message;
        game.Messages.Add(new Message(aiMessage.Content, aiMessage.Role) { Game = game });
        game.AiRecords.Add(new AiRecord(aiMessage.Role, aiMessage.Content) { Game = game });
        await gameRepository.Update(game, ct);
        await Clients.Caller.SendAsync(
            "ReceiveMessage",
            new
            {
                role = aiMessage.Role,
                content = aiMessage.Content,
                creationDate = Instant
                    .FromDateTimeUtc(DateTime.Now.ToUniversalTime())
                    .InZone(DateTimeZoneProviders.Tzdb[request.TimeZone])
                    .ToDateTimeUnspecified()
                    .ToString("HH:mm")
            },
            ct);
        var userPrompt2 = promptCreator.GetThreeAnswers(aiMessage.Content, request.Language);
        var systemPrompt = promptCreator.GameSystemContent();
        var requestData2 = new AiRequest
        {
            Model = "GigaChat-Plus",
            Messages =
            [
                new AiMessage { Role = "system", Content = systemPrompt },
                new AiMessage { Role = "user", Content = userPrompt2 }
            ]
        };
        var jsonRequestData2 = JsonSerializer.Serialize(requestData2, _jsonOptions);
        while (aiAssistant.QueueCount != 0)
        {
            await Clients.Caller.SendAsync("ReceiveQueueCount", new { count = aiAssistant.QueueCount }, ct);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        await Clients.Caller.SendAsync("ReceiveMessageInProcess", ct);
        var response2 = await aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData2, httpClient, ct);
        var aiMessage2 = response2.Choices[0].Message;
        var data = JsonSerializer.Deserialize<ChoiceResponse>(aiMessage2.Content);
        await Clients.Caller.SendAsync("ReceiveChoices", data, ct);
    }

    [SignalRMethod]
    [Authorize]
    public async Task StartGame(
        StartGameRequest request,
        [SignalRHidden] [FromServices] IHttpClientFactory httpClientFactory,
        [SignalRHidden] [FromServices] IAiAssistant aiAssistant,
        [SignalRHidden] [FromServices] PromptCreator promptCreator,
        [SignalRHidden] [FromServices] IGameRepository gameRepository,
        [SignalRHidden] [FromServices] IUserRepository userRepository,
        [SignalRHidden] [FromServices] CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(aiAssistant);
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        ArgumentNullException.ThrowIfNull(promptCreator);
        ArgumentNullException.ThrowIfNull(gameRepository);

        var startGamePrompt = promptCreator.StartGame(request.CharName, request.Location, request.Language);
        var systemPrompt = promptCreator.GameSystemContent();
        var game = await gameRepository.GetById(request.GameId, ct);

        game.Messages.Add(new Message(request.Location, "user") { Game = game });
        game.AiRecords.Add(new AiRecord("system", systemPrompt) { Game = game });
        game.AiRecords.Add(new AiRecord("user", startGamePrompt) { Game = game });
        var requestData = new AiRequest
        {
            Model = "GigaChat-Plus",
            Messages = game.AiRecords
                .Select(e => new AiMessage
                {
                    Role = e.Role,
                    Content = e.Content
                })
                .ToList()
        };
        var jsonRequestData = JsonSerializer.Serialize(requestData, _jsonOptions);
        var httpClient = httpClientFactory.CreateClient("GigaChat");
        while (aiAssistant.QueueCount != 0)
        {
            await Clients.Caller.SendAsync("ReceiveQueueCount", new { count = aiAssistant.QueueCount }, ct);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        await Clients.Caller.SendAsync("ReceiveMessageInProcess", ct);
        var response = await aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData, httpClient, ct);
        var aiMessage = response.Choices[0].Message;
        game.Messages.Add(new Message(aiMessage.Content, aiMessage.Role) { Game = game });
        game.AiRecords.Add(new AiRecord(aiMessage.Role, aiMessage.Content) { Game = game });
        game.GameState = GameState.InProcess;
        await gameRepository.Update(game, ct);
        await Clients.Caller.SendAsync("ReceiveGameState", game.GameState, ct);
        await Clients.Caller.SendAsync(
            "ReceiveMessage",
            new
            {
                role = aiMessage.Role,
                content = aiMessage.Content,
                creationDate = Instant
                    .FromDateTimeUtc(DateTime.Now.ToUniversalTime())
                    .InZone(DateTimeZoneProviders.Tzdb[request.TimeZone])
                    .ToDateTimeUnspecified()
                    .ToString("HH:mm")
            },
            ct);
        var userPrompt2 = promptCreator.GetThreeAnswers(aiMessage.Content, request.Language);
        var requestData2 = new AiRequest
        {
            Model = "GigaChat-Plus",
            Messages =
            [
                new AiMessage { Role = "system", Content = systemPrompt },
                new AiMessage { Role = "user", Content = userPrompt2 }
            ]
        };
        var jsonRequestData2 = JsonSerializer.Serialize(requestData2, _jsonOptions);
        while (aiAssistant.QueueCount != 0)
        {
            await Clients.Caller.SendAsync("ReceiveQueueCount", new { count = aiAssistant.QueueCount }, ct);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        await Clients.Caller.SendAsync("ReceiveMessageInProcess", ct);
        var response2 = await aiAssistant.ExecuteRequest<AiResponse>(jsonRequestData2, httpClient, ct);
        var aiMessage2 = response2.Choices[0].Message;
        var data = JsonSerializer.Deserialize<ChoiceResponse>(aiMessage2.Content);
        await Clients.Caller.SendAsync("ReceiveChoices", data, ct);
    }

    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Notify", new { message = $"{Context.ConnectionId} joined the chat" });
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (exception != null)
        {
            await Clients.Caller.SendAsync("Retry", new { exception = exception.Message });
        }

        await Clients.Caller.SendAsync("Notify", new { message = $"{Context.ConnectionId} leaved the chat" });
        await base.OnDisconnectedAsync(exception);
    }
}