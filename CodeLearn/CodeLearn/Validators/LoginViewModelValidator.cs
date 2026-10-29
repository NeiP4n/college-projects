using FluentValidation;

namespace CodeLearn.Validators
{
    public class LoginViewModelValidator : AbstractValidator<LoginViewModel>
    {
        public LoginViewModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Введите email")
                .EmailAddress().WithMessage("Неверный формат email");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Введите пароль");
        }
    }
}
