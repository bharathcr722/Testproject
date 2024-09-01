using System.ComponentModel.DataAnnotations;

namespace Testproject.Models.EntityModel
{
    public class UserDetails
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [MinLength(4)]
        public string PasswordHash { get; set; }
        public byte[] Salt { get; set; }
        
    }
}
