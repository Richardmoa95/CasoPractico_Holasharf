using FluentValidation;
using IntegrationServices.API.Contracts;
using IntegrationServices.API.Validators;
using IntegrationServices.Application.Events;
using IntegrationServices.Infrastructure;
using IntegrationServices.Worker;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddValidatorsFromAssemblyContaining<TmsWebhookEventRequestValidator>();

builder.Services.AddScoped<ProcessTmsEventUseCase>();

builder.Services.AddInfrastructure();
builder.Services.AddWorker();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.MapControllers();

app.MapGet("/", () => new
{
    app = "TMS Integration API",
    status = "running",
    endpoints = new[]
    {
        "POST /api/webhooks/tms/events",
        "GET /api/orders/{orderNumber}",
        "GET /api/orders/{orderNumber}/history",
        "GET /api/dead-letter"
    }
});

app.Run();