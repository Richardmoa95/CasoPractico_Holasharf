using FluentValidation;
using IntegrationServices.API.Contracts;

namespace IntegrationServices.API.Validators;

public sealed class TmsWebhookEventRequestValidator : AbstractValidator<TmsWebhookEventRequest>
{
    public TmsWebhookEventRequestValidator()
    {
        RuleFor(x => x.ServiceType)
            .NotEmpty();

        RuleFor(x => x.DispatchType)
            .NotEmpty();

        RuleFor(x => x.Status)
            .NotEmpty();

        RuleFor(x => x.EventDate)
            .NotEmpty();

        RuleFor(x => x.Details)
            .NotNull();

        RuleFor(x => x.Details.OrderNumber)
            .NotEmpty()
            .When(x => x.Details is not null);

        RuleFor(x => x.Details.TrackingNumber)
            .NotEmpty()
            .When(x => x.Details is not null);

        RuleFor(x => x.Details.ClientCode)
            .NotEmpty()
            .When(x => x.Details is not null);

        RuleFor(x => x.Details.ClientName)
            .NotEmpty()
            .When(x => x.Details is not null);
    }
}