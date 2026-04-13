using FluentValidation;
using SecurityReport.Application.Commands;

namespace SecurityReport.Application.Validators
{
    public class CreateReportCommandValidator : AbstractValidator<CreateReportCommand>
    {
        public CreateReportCommandValidator()
        {
            RuleFor(x => x.Titulo).NotEmpty().MaximumLength(500);
            RuleFor(x => x.Descripcion).NotEmpty();
            RuleFor(x => x.AreaId).NotEmpty();
            RuleFor(x => x.EstadoReporteId).NotEmpty();
            RuleFor(x => x.ReportadoPorId).NotEmpty();
        }
    }
}