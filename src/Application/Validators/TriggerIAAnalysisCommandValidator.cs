using FluentValidation;
using SecurityReport.Application.Commands;

namespace SecurityReport.Application.Validators
{
    public class TriggerIAAnalysisCommandValidator : AbstractValidator<TriggerIAAnalysisCommand>
    {
        public TriggerIAAnalysisCommandValidator()
        {
            RuleFor(x => x.ReporteId).NotEmpty();
            RuleFor(x => x.Tipo).NotEmpty();
        }
    }
}