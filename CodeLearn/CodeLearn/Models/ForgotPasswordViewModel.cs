using System.ComponentModel.DataAnnotations;

namespace CodeLearn.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "Укажи email")]
        [EmailAddress(ErrorMessage = "Некорректный email")]
        public string Email { get; set; } = "";
    }
}
