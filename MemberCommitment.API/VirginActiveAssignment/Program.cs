
using Microsoft.Extensions.Http.Resilience;
using Microsoft.OpenApi;
using Polly;
using System.Text.Json.Serialization;
using VirginActiveAssignment;
using VirginActiveAssignment.Middleware;
using VirginActiveAssignment.Models;
using VirginActiveAssignment.Services;
using VirginActiveAssignment.Services.Factory;
using VirginActiveAssignment.Services.Validation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        Name = "X-Api-Key",
        In = ParameterLocation.Header,
        Description = "Enter your API key."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("ApiKey", document),
            new List<string>()
        }
    });
});

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(options =>
{
    options.IncludeScopes = true;
});
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ExceptionHandler>();

builder.Services.AddSingleton<IValidation, OtherValidator>();
builder.Services.AddSingleton<IValidation, RevenueValidator>();
builder.Services.AddSingleton<IValidation, HealthValidator>();
builder.Services.AddSingleton<IValidation, CareerValidator>();
builder.Services.AddSingleton<RockValidatorFactory>();
builder.Services
    .AddHttpClient<MemberService>(client =>
    {
        client.BaseAddress = new Uri(
            "https://jsonplaceholder.typicode.com/");

        client.Timeout = TimeSpan.FromSeconds(10);
    })
    .AddResilienceHandler("ProfileRetry", (pipeline, context) =>
    {
        var logger = context.ServiceProvider
            .GetRequiredService<ILogger<MemberService>>();

        pipeline.AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,

            OnRetry = args =>
            {
                var reason =
                    args.Outcome.Exception?.Message
                    ?? args.Outcome.Result?.StatusCode.ToString()
                    ?? "Unknown";

                logger.LogWarning(
                    "Retrying profile request. Attempt {AttemptNumber}, Delay {Delay}, Reason {Reason}",
                    args.AttemptNumber,
                    args.RetryDelay,
                    reason);

                return default;
            }
        });
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();


app.Run();
