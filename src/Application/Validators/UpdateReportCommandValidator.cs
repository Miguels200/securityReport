using FluentValidation;
using SecurityReport.Application.Commands;

namespace SecurityReport.Application.Validators
{
    public class UpdateReportCommandValidator : AbstractValidator<UpdateReportCommand>
    {
        public UpdateReportCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleFor(x => x.Titulo).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Descripcion).NotEmpty();
            RuleFor(x => x.Observaciones).MaximumLength(4000);
            RuleFor(x => x.EstadoReporteId).NotEmpty();
        }
    }
}