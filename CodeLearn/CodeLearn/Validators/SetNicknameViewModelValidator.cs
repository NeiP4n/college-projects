using FluentValidation;

namespace CodeLearn.Validators
{
    public class SetNicknameViewModelValidator : AbstractValidator<SetNicknameViewModel>
    {
        public SetNicknameViewModelValidator()
        {
            RuleFor(x => x.NickName)
                .NotEmpty().WithMessage("Введите никнейм")
                .MinimumLength(3).WithMessage("Минимум 3 символа")
                .MaximumLength(20).WithMessage("Максимум 20 символов");
        }
    }
}
