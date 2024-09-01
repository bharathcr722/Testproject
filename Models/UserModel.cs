using System.ComponentModel.DataAnnotations;

namespace Testproject.Models
{
    public class UserModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(4)]
        public string Password { get; set; }
        [Required]
        [MinLength(4)]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public string RoleName { get; set; }

    }
    public class LoginModel
    { 
        [Required(ErrorMessage ="Email is required..")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [MinLength(4)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
