using FluentValidation;
using SecurityReport.Application.Commands;

namespace SecurityReport.Application.Validators
{
    public class CreateNormativaCommandValidator : AbstractValidator<CreateNormativaCommand>
    {
        public CreateNormativaCommandValidator()
        {
            RuleFor(x => x.Codigo).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Titulo).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Contenido).NotEmpty();
        }
    }
}