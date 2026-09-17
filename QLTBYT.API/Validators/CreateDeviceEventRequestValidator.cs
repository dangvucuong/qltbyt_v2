using FluentValidation;
using QLTBYT.API.Models.DTOs.Event;

namespace QLTBYT.API.Validators;

public class CreateDeviceEventRequestValidator : AbstractValidator<CreateDeviceEventRequest>
{
    public CreateDeviceEventRequestValidator()
    {
        RuleFor(x => x.EventTypeId).InclusiveBetween(1, 4);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(500);
        RuleFor(x => x.Keyword).MaximumLength(500);
    }
}
