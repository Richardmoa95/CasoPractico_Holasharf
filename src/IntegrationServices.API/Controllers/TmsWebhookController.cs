using FluentValidation;
using IntegrationServices.API.Contracts;
using IntegrationServices.API.Mappers;
using IntegrationServices.Application.Ports;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationServices.API.Controllers;

[ApiController]
[Route("api/webhooks/tms")]
public sealed class TmsWebhookController : ControllerBase
{
    private readonly IMessageBus _messageBus;
    private readonly IValidator<TmsWebhookEventRequest> _validator;
    private readonly ILogger<TmsWebhookController> _logger;

    public TmsWebhookController(IMessageBus messageBus, IValidator<TmsWebhookEventRequest> validator, ILogger<TmsWebhookController> logger)
    {
        _messageBus = messageBus;
        _validator = validator;
        _logger = logger;
    }

    [HttpPost("events")]
    public async Task<IActionResult> ReceiveEvent([FromBody] TmsWebhookEventRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
        {
            return BadRequest(new
            {
                message = "Invalid webhook payload.",
                errors = validationResult.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                })
            });
        }

        try
        {
            var message = TmsWebhookMapper.ToMessage(request);

            await _messageBus.PublishAsync(message, cancellationToken);

            _logger.LogInformation("TMS webhook accepted. EventId: {EventId}, OrderNumber: {OrderNumber}, Status: {Status}", message.EventId, message.OrderNumber, message.Status);

            return Accepted(new
            {
                message = "Webhook accepted for asynchronous processing.",
                eventId = message.EventId,
                orderNumber = message.OrderNumber,
                status = message.Status.ToString()
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new
            {
                message = exception.Message
            });
        }
    }
}