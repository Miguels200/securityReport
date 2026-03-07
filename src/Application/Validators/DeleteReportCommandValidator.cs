using FluentValidation;
using SecurityReport.Application.Commands;

namespace SecurityReport.Application.Validators
{
    public class DeleteReportCommandValidator : AbstractValidator<DeleteReportCommand>
    {
        public DeleteReportCommandValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
        }
    }
}