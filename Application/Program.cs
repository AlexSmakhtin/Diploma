using Application.Configurations;
using Application.Services.Implementations;
using Application.Services.Interfaces;
using Application.SignalR.Hub;
using Domain.Repositories.Interfaces;
using Domain.Services.Implementations;
using Domain.Services.Interfaces;
using Infrastructure.DbContext;
using Infrastructure.Repositories.Implementations;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Protocols.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Application;

public class Program
{
    public static async Task Main()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();
        Log.Information("Ai Adventure Server starting...");
        try
        {
            var builder = WebApplication.CreateBuilder();
            builder.Host.UseSerilog((hbc, conf) =>
            {
                conf.MinimumLevel.Information()
                    .WriteTo.Console()
                    .MinimumLevel.Information();
            });
            builder.Configuration.AddUserSecrets<Program>();
            JwtConfig jwtConfig;
            ConnectionStrings connectionStrings;
            GigaChatData gigaChatData;
            EmailSenderConfig emailSenderConfig;

            var amazonS3Config = builder.Configuration
                .GetRequiredSection("AmazonS3Config")
                .Get<AmazonS3Config>();
            if (amazonS3Config is null)
            {
                throw new InvalidOperationException("AmazonS3Config is not configured");
            }

            using (var s3Manager = new VkS3Manager(
                       amazonS3Config.S3Url1,
                       amazonS3Config.AccessKey,
                       amazonS3Config.SecretKey,
                       amazonS3Config.S3Url2))
            {
                jwtConfig = await s3Manager.GetFileAsObject<JwtConfig>(
                    amazonS3Config.JwtConfigFileName,
                    amazonS3Config.BucketName,
                    CancellationToken.None);
                connectionStrings = await s3Manager.GetFileAsObject<ConnectionStrings>(
                    amazonS3Config.ConnStrFileName,
                    amazonS3Config.BucketName,
                    CancellationToken.None);
                gigaChatData = await s3Manager.GetFileAsObject<GigaChatData>(
                    amazonS3Config.GigaChatDataFileName,
                    amazonS3Config.BucketName,
                    CancellationToken.None);
                emailSenderConfig = await s3Manager.GetFileAsObject<EmailSenderConfig>(
                    amazonS3Config.EmailSenderConfigFileName,
                    amazonS3Config.BucketName,
                    CancellationToken.None);
            }

            builder.Services.Configure<EmailSenderConfig>(config =>
            {
                config.Host = emailSenderConfig.Host;
                config.Login = emailSenderConfig.Login;
                config.Password = emailSenderConfig.Password;
                config.Port = emailSenderConfig.Port;
            });
            builder.Services.Configure<GigaChatData>(config =>
            {
                config.AuthUrl = gigaChatData.AuthUrl;
                config.ChatUrl = gigaChatData.ChatUrl;
                config.Base64AuthData = gigaChatData.Base64AuthData;
            });
            builder.Services.Configure<JwtConfig>(config =>
            {
                config.SigningKey = jwtConfig.SigningKey;
                config.Audience = jwtConfig.Audience;
                config.Issuer = jwtConfig.Issuer;
                config.LifeTime = jwtConfig.LifeTime;
            });
#pragma warning disable CA1416
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo(@"C:\Users\lokom\AppData\Local\ASP.NET\DataProtection-Keys"))
                .ProtectKeysWithDpapi();
#pragma warning restore CA1416
            builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        IssuerSigningKey = new SymmetricSecurityKey(jwtConfig.SigningKeyBytes),
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidAudiences = new[] { jwtConfig.Audience },
                        ValidIssuer = jwtConfig.Issuer
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            builder.Services.AddAuthorization();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. " +
                                  "\r\n\r\n Enter 'Bearer' [space] and then your token in the text input below." +
                                  "\r\n\r\nExample: \"Bearer {token}\"",
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
                options.AddSignalRSwaggerGen(action =>
                {
                    action.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });
            });
            builder.Services.AddDbContext<PostgreDbContext>(options =>
            {
                var connectionString = connectionStrings.PostgreSql;
                if (builder.Environment.IsDevelopment())
                    connectionString = builder.Configuration.GetConnectionString("PostgreSql");
                options.UseNpgsql(connectionString,
                    b => b.MigrationsAssembly("Application"));
            });

            builder.Services.AddScoped(typeof(IRepository<>), typeof(PostgreRepository<>));
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IGameRepository, GameRepository>();
            builder.Services.AddScoped<IEmailSender, EmailSender>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<PromptCreator>();
            builder.Services.AddSingleton<IAiAssistant, GigaChatAiAssistant>();
            builder.Services.AddSingleton<IPasswordHasherService, PasswordHasherService>();
            builder.Services.AddSingleton<IJwtService, JwtService>();
            builder.Services.AddSignalR()
                .AddHubOptions<ChatHub>(options =>
                {
                    options.EnableDetailedErrors = true;
                    options.ClientTimeoutInterval = TimeSpan.FromMinutes(30);
                    options.HandshakeTimeout = TimeSpan.FromMinutes(1);
                });
            builder.Services.AddHttpClient("GigaChat")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                });
            builder.Services.AddHttpClient("Voiceover")
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                });
            builder.Services.AddMemoryCache();
            builder.Services.AddCors();
            builder.Services.AddHttpLogging(options =>
            {
                options.LoggingFields = HttpLoggingFields.RequestHeaders
                                        | HttpLoggingFields.ResponseHeaders
                                        | HttpLoggingFields.RequestBody
                                        | HttpLoggingFields.ResponseBody;
            });
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1"); });
            app.UseCors(policy =>
            {
                policy
                    .AllowCredentials()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithOrigins("https://fadventure.fun", "http://localhost:5173");
            });
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpLogging();
            app.MapControllers();
            app.MapHub<ChatHub>("/chat")
                .RequireCors(policy =>
                {
                    policy
                        .AllowCredentials()
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins("https://fadventure.fun", "http://localhost:5173");
                });
            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unexpected error");
        }
        finally
        {
            Log.Information("Server shutting down");
            await Log.CloseAndFlushAsync();
        }
    }
}