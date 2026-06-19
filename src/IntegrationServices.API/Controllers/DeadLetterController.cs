using IntegrationServices.Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace IntegrationServices.API.Controllers;

[ApiController]
[Route("api/dead-letter")]
public sealed class DeadLetterController : ControllerBase
{
    private readonly DeadLetterQueue _deadLetterQueue;

    public DeadLetterController(DeadLetterQueue deadLetterQueue)
    {
        _deadLetterQueue = deadLetterQueue;
    }

    [HttpGet]
    public IActionResult GetAll()
    {
        var messages = _deadLetterQueue.GetAll();

        return Ok(messages.Select(x => new
        {
            x.Message.EventId,
            x.Message.OrderNumber,
            status = x.Message.Status.ToString(),
            x.Error,
            x.FailedAtUtc
        }));
    }
}