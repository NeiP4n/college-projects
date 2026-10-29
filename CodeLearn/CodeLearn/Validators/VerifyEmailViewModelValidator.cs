using FluentValidation;

namespace CodeLearn.Validators
{
    public class VerifyEmailViewModelValidator : AbstractValidator<VerifyEmailViewModel>
    {
        public VerifyEmailViewModelValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Введите код")
                .Length(6).WithMessage("Код должен быть из 6 цифр")
                .Matches(@"^\d{6}$").WithMessage("Код должен состоять из цифр");
        }
    }
}
