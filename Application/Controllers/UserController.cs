using System.Security.Claims;
using System.Text;
using Application.DTO_s.ControllersDTO_s.Requests;
using Application.DTO_s.ControllersDTO_s.Responses;
using Domain.Enums;
using Domain.Exceptions;
using Domain.Repositories.Interfaces;
using Domain.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace Application.Controllers;

[Route("api/user")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;
    private readonly IUserRepository _userRepository;
    private readonly IGameRepository _gameRepository;
    private readonly IEmailSender _emailSender;
    private readonly IJwtService _jwtService;

    public UserController(
        IUserService userService,
        ILogger<UserController> logger,
        IUserRepository userRepository,
        IGameRepository gameRepository,
        IEmailSender emailSender,
        IJwtService jwtService)
    {
        ArgumentNullException.ThrowIfNull(jwtService);
        ArgumentNullException.ThrowIfNull(emailSender);
        ArgumentNullException.ThrowIfNull(userService);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(userRepository);
        ArgumentNullException.ThrowIfNull(gameRepository);
        _userService = userService;
        _logger = logger;
        _userRepository = userRepository;
        _gameRepository = gameRepository;
        _emailSender = emailSender;
        _jwtService = jwtService;
    }

    [Authorize]
    [HttpGet]
    public async Task<ActionResult<UserGetResponse>> GetUser(CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(strId);
        var user = await _userRepository.GetById(userId, ct);
        return new UserGetResponse(user.Name, user.Email, user.Status);
    }

    [Authorize]
    [HttpPost("change_password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request,
        CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(strId);
        var user = await _userRepository.GetById(userId, ct);
        await _userService.ChangePassword(user, request.OldPassword, request.NewPassword, ct);
        return Ok();
    }

    [Authorize]
    [HttpPost("update")]
    public async Task<ActionResult<UserUpdateResponse>> UpdateUser([FromBody] UserUpdateRequest request,
        CancellationToken ct)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(strId);
        var user = await _userRepository.GetById(userId, ct);
        user.Name = request.Name;
        if (request.Email != user.Email)
        {
            var existedUser = await _userRepository.FindByEmail(user.Email, ct);
            if (existedUser != null)
            {
                return BadRequest("Email already used");
            }
            user.Email = request.Email;
            user.Status = Statuses.Common;
            var origin = Request.Headers.Origin.ToString();
            var emailToken = _jwtService.GenerateEmailToken(user);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailToken));
            var url = $"{origin}/confirm_email/{token}";
            var subject = "AiAdventure. Confirm your email";
            var body = $"Please confirm your account by clicking <a href='{url}'>here</a>.";
            await _emailSender.SendEmail(user.Email, subject, body, ct);
        }

        await _userRepository.Update(user, ct);
        var newToken = _jwtService.GenerateToken(user);
        return new UserUpdateResponse(user.Name, user.Email, user.Status, newToken);
    }

    [HttpPost("sign_in")]
    public async Task<ActionResult<SignInResponse>> SignIn(
        [FromBody] SignInRequest request,
        [FromServices] IJwtService jwtService,
        CancellationToken ct)
    {
        try
        {
            var existedUser = await _userService.SignIn(
                request.Email,
                request.Password,
                ct);
            var jwt = jwtService.GenerateToken(existedUser);
            return new SignInResponse(existedUser.Id, existedUser.Name, existedUser.Email, jwt, existedUser.Status);
        }
        catch (UserNotFoundException)
        {
            return Unauthorized("User not found");
        }
        catch (IncorrectPasswordException)
        {
            return Unauthorized("Incorrect password");
        }
    }

    [HttpPost("sign_up")]
    public async Task<ActionResult<SignUpResponse>> Register(
        [FromBody] SignUpRequest request,
        CancellationToken ct)
    {
        try
        {
            var newUser = await _userService.SignUp(
                request.Name,
                request.Email,
                request.Password,
                request.Status,
                request.Birthday,
                ct);
            var origin = Request.Headers.Origin.ToString();
            var emailToken = _jwtService.GenerateEmailToken(newUser);
            var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailToken));
            var url = $"{origin}/confirm_email/{token}";
            var subject = "AiAdventure. Confirm your email";
            var body = $"Please confirm your account by clicking <a href='{url}'>here</a>.";
            await _emailSender.SendEmail(newUser.Email, subject, body, ct);
            var jwt = _jwtService.GenerateToken(newUser);
            return new SignUpResponse(newUser.Id, newUser.Name, newUser.Email, jwt, newUser.Status);
        }
        catch (UserEmailAlreadyExistsException)
        {
            return BadRequest("User email already used");
        }
    }

    [Authorize]
    [HttpGet("retry_confirm_email")]
    public async Task<IActionResult> RetryConfirmEmail(CancellationToken ct)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(idStr);
        var user = await _userRepository.GetById(userId, ct);
        var origin = Request.Headers.Origin.ToString();
        var emailToken = _jwtService.GenerateEmailToken(user);
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes(emailToken));
        var url = $"{origin}/confirm_email/{token}";
        var subject = "AiAdventure. Confirm your email";
        var body = $"Please confirm your account by clicking <a href='{url}'>here</a>.";
        await _emailSender.SendEmail(user.Email, subject, body, ct);
        return Ok("Retry success");
    }

    [HttpGet("confirm_email")]
    public async Task<ActionResult<SignUpResponse>> ConfirmEmail(
        [FromQuery] string token,
        CancellationToken ct)
    {
        var emailToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var claims = _jwtService.ValidateEmailToken(emailToken);
        var userId = claims.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = claims.FindFirstValue(ClaimTypes.Email);
        if (userId == null)
            return BadRequest("Invalid token (user id)");
        if (email == null)
            return BadRequest("Invalid token (user email)");
        var userGuidId = Guid.Parse(userId);
        var user = await _userRepository.GetById(userGuidId, ct);
        if (user.Email != email)
        {
            return BadRequest("Invalid token (email does not match)");
        }

        user.Status = Statuses.Confirmed;
        await _userRepository.Update(user, ct);
        var newToken = _jwtService.GenerateToken(user);
        return new SignUpResponse(user.Id, user.Name, user.Email, newToken, user.Status);
    }

    [Authorize]
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok();
    }

    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpGet("history")]
    public async Task<List<GameHistoryResponse>> GetHistory(
        [FromQuery] int currentPage,
        [FromHeader(Name = "Time-Zone")] string timeZone,
        CancellationToken ct,
        [FromQuery] int takeCount = 10)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(idStr);
        var user = await _userRepository.GetById(userId, ct);
        var games = await _gameRepository.GetGamesByUserId(user.Id, ct);
        var response = games
            .OrderByDescending(e => e.CreationDate)
            .Skip((currentPage - 1) * takeCount)
            .Take(takeCount)
            .Select(e => new GameHistoryResponse(
                e.Id,
                e.Name,
                Instant
                    .FromDateTimeUtc(e.CreationDate)
                    .InZone(DateTimeZoneProviders.Tzdb[timeZone])
                    .ToDateTimeUnspecified()
                    .ToString("MM/dd/yyyy HH:mm"),
                e.Messages.Count,
                e.GameState.ToString()))
            .ToList();
        return response;
    }


    [Authorize(Roles = nameof(Statuses.Confirmed))]
    [HttpGet("history_pages_count")]
    public async Task<ActionResult<HistoryTotalPagesResponse>> GetHistoryPagesCount(
        CancellationToken ct,
        [FromQuery] int takeCount = 10)
    {
        var strId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? throw new InvalidOperationException("User not found");
        var userId = Guid.Parse(strId);
        var gameCount = await _gameRepository.GetTotalGamesCount(userId, ct);
        var pagesCount = (int)Math.Ceiling((double)gameCount / takeCount);
        return new HistoryTotalPagesResponse(pagesCount);
    }
}