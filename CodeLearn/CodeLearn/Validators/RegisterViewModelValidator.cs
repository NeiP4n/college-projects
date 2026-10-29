using FluentValidation;

namespace CodeLearn.Validators
{
    public class RegisterViewModelValidator : AbstractValidator<RegisterViewModel>
    {
        public RegisterViewModelValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Введите email")
                .EmailAddress().WithMessage("Неверный формат email");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Введите пароль")
                .MinimumLength(6).WithMessage("Минимум 6 символов");

            RuleFor(x => x.ConfirmPassword)
                .Equal(x => x.Password).WithMessage("Пароли не совпадают");
        }
    }
}
