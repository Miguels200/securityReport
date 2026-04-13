using FluentValidation;
using SecurityReport.Application.Commands;

namespace SecurityReport.Application.Validators
{
    public class CreateRiesgoCommandValidator : AbstractValidator<CreateRiesgoCommand>
    {
        public CreateRiesgoCommandValidator()
        {
            RuleFor(x => x.Descripcion).NotEmpty();
            RuleFor(x => x.Ocurrencias).GreaterThanOrEqualTo(1);
            RuleFor(x => x.NivelRiesgo).NotEmpty();
        }
    }
}