using System.ComponentModel.DataAnnotations;

namespace CodeLearn.Models
{
    public class ResetPasswordViewModel
    {
        [Required] public string Email { get; set; } = "";
        [Required] public string Token { get; set; } = "";

        [Required(ErrorMessage = "Введи новый пароль")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль минимум 6 символов")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают")]
        public string ConfirmPassword { get; set; } = "";
    }
}
